﻿using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyAnalyzer.Windsor
{
   class ControllerInstaller : IWindsorInstaller
   {
      public void Install(IWindsorContainer container, IConfigurationStore store)
      {
         container.Register(Classes.FromThisAssembly()
                 .Pick().If(t => t.Name.EndsWith("Controller"))
                 .Configure(conf => conf.Named(conf.Implementation.Name))
                 .LifestyleScoped());
      }
   }
}
