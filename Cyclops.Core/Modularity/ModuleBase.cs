﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;

namespace Cyclops.Core.Modularity
{
    public abstract class ModuleBase : IModule
    {
        #region IModule Members

        public virtual IEnumerable<IModule> Children
        {
            get { return Enumerable.Empty<IModule>(); }
        }

        public virtual void Initialize(IUnityContainer container)
        {
        }

        #endregion
    }
}