﻿using FubuMVC.Core;
using FubuMVC.Core.Continuations;
using FubuMVC.Core.Registration;
using NUnit.Framework;
using FubuMVC.StructureMap;
using StructureMap;
using FubuMVC.Katana;
using FubuTestingSupport;

namespace FubuMVC.IntegrationTesting.Continuations
{
    [TestFixture]
    public class can_use_continuations_on_a_partial
    {
        [Test]
        public void transfer_in_a_partial()
        {
            using (var server = FubuApplication.DefaultPolicies().StructureMap(new Container()).RunEmbedded(port:5510))
            {
                server.Endpoints.GetByInput(new FullInput {Redirect = false})
                      .ReadAsText().ShouldEqual("original");

                server.Endpoints.GetByInput(new FullInput { Redirect = true })
                    .ReadAsText().ShouldEqual("transferred");

            }
        }
    }

    public class PartialInput
    {
        public bool Redirect { get; set; }    
    }

    public class FullInput
    {
        public bool Redirect { get; set; }
    }

    public class PartialFilter
    {
        public FubuContinuation FilterInput(PartialInput input)
        {
            if (input.Redirect)
            {
                return FubuContinuation.TransferTo<PartialEndpoints>(x => x.get_filtered_transferred());
            }

            return FubuContinuation.NextBehavior();
        }
    }

    public class PartialEndpoints
    {
        [Filter(typeof(PartialFilter))]
        public FubuContinuation get_full_method_Redirect(FullInput input)
        {
            return FubuContinuation.TransferTo(new PartialInput {Redirect = input.Redirect});
        }

        [FubuPartial]
        public string get_filtered_partial(PartialInput input)
        {
            return "original";
        }

        [FubuPartial]
        public string get_filtered_transferred()
        {
            return "transferred";
        }
    }


}