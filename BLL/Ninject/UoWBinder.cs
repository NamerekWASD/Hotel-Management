﻿using Ninject.Modules;
using UnitsOfWork;
using UnitsOfWork.Interfaces;

namespace BLL.Ninject
{
    public class UoWBinder : NinjectModule
    {
        public override void Load()
        {
            Bind<IUnitOfWork>().To<UnitOfWork>();
        }
    }
}
