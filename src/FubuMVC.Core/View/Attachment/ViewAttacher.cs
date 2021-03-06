using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FubuCore;
using FubuCore.Reflection;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Registration.Routes;
using FubuMVC.Core.Resources.Conneg;
using FubuMVC.Core.Runtime.Conditionals;

namespace FubuMVC.Core.View.Attachment
{

    public class ViewAttachmentWorker
    {
        private readonly ViewBag _views;
        private readonly ViewAttachmentPolicy _policy;
        private readonly IList<IViewToken> _attached = new List<IViewToken>(); 

        public ViewAttachmentWorker(ViewBag views, ViewAttachmentPolicy policy)
        {
            _views = views;
            _policy = policy;
        }

        public void Configure(BehaviorGraph graph)
        {
            if (!_views.Views.Any()) return;

            FindLastActions(graph.Behaviors).Each(attachToAction);

            _views.Views.Where(x => x.ViewModel != null && !_attached.Contains(x)).Each(view => {
                var chain = buildChainForView(view);
                chain.Output.AddView(view, Always.Flyweight);

                graph.AddChain(chain);
            });
        }

        private BehaviorChain buildChainForView(IViewToken view)
        {
            if (view.ViewModel.HasAttribute<UrlPatternAttribute>())
            {
                var url = view.ViewModel.GetAttribute<UrlPatternAttribute>().Pattern;
                return new RoutedChain(new RouteDefinition(url), view.ViewModel, view.ViewModel);
            }

            return BehaviorChain.ForResource(view.ViewModel);
        }

        private void attachToAction(ActionCall action)
        {
            _policy.Profiles(_views).Each(x => Attach(x.Profile, x.Views, action));
        }

        public void Attach(IViewProfile viewProfile, ViewBag bag, ActionCall action)
        {
            // No duplicate views!
            var outputNode = action.ParentChain().Output;
            if (outputNode.HasView(viewProfile.Condition)) return;


            foreach (var filter in _policy.Filters())
            {
                var viewTokens = filter.Apply(action, bag);
                var count = viewTokens.Count();

                if (count != 1) continue;

                var token = viewTokens.Single().Resolve();

                _attached.Add(token);
                outputNode.AddView(token, viewProfile.Condition);

                break;
            }
        }



        public static IEnumerable<ActionCall> FindLastActions(IEnumerable<BehaviorChain> chains)
        {
            foreach (var chain in chains)
            {
                var last = chain.Calls.LastOrDefault();
                if (last != null && last.HasOutput)
                {
                    yield return last;
                }
            }
        }
    }

}