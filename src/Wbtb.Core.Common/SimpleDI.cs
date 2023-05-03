using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection;

namespace Wbtb.Core.Common
{

    public interface ISimpleDIFactory
    {
        object Resolve<T>();
    }

    /// <summary>
    /// A very simple dependency injection system in a single file.
    /// </summary>
    public class SimpleDI
    {
        private delegate object CompiledConstructor(params object[] args);

        private static IList<Registration> ApplicationContextRegister = new List<Registration>();

        private static Dictionary<Type, CompiledConstructor> ApplicationContextConstructors = new Dictionary<Type, CompiledConstructor>();

        class Registration
        {
            /// <summary>
            /// Service or interface type is registered by.
            /// </summary>
            public Type Service { get; set; }

            /// <summary>
            /// The concrete type that fulfills the service requirement. Replaced by Factory or Singleton.
            /// </summary>
            public Type Implementation { get; set; }

            /// <summary>
            /// A factory that provides an implementation. Replaced by Implementation or Singleton.
            /// </summary>
            public Type Factory { get; set; }

            /// <summary>
            /// A global instance that fulfills service. Replaced by Implementation or Factory.
            /// </summary>
            public object Singleton { get; set; }
        }

        /// <summary>
        /// Registered service-implementation combinations.
        /// </summary>
        private IList<Registration> _register = new List<Registration>();

        /// <summary>
        /// Caches compile constructors
        /// </summary>
        private Dictionary<Type, CompiledConstructor> _constructors = new Dictionary<Type, CompiledConstructor>();

        #region CTORS

        public SimpleDI()
        {
            _register = ApplicationContextRegister;
            _constructors = ApplicationContextConstructors;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="service"></param>
        /// <param name="implementation"></param>
        /// <param name="allowMultiple"></param>
        public void Register<TService, TImplementation>(bool allowMultiple = false)
        {
            Register(typeof(TService), typeof(TImplementation), allowMultiple);
        }

        public void RegisterFactory<TService, TFactory>() 
        {
            lock (_register)
            {
                Type factory = typeof(TFactory);
                Type service = typeof(TService);
                if (!typeof(ISimpleDIFactory).IsAssignableFrom(factory))
                    throw new Exception($"Factory type {factory.Name} does not implement {typeof(ISimpleDIFactory).Name}.");

                if (_register.Where(r => r.Service == service).Any())
                    throw new Exception($"Cannot bind service type {TypeHelper.Name(service)}, a binding for this already exists.");

                // register factory against itself, as we need to create instance of this to provide service
                if (!_register.Any(r => r.Service == factory))
                    _register.Add(new Registration { Service = factory, Implementation = factory });

                // register factory against service 
                _register.Add(new Registration { Service = service, Factory = factory });
            }
        }

        /// <summary>
        /// Binds an implementation to a service type. Registration is required before resolving.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="implementation"></param>
        /// <param name="allowMultiple"></param>
        /// <exception cref="Exception"></exception>
        public void Register(Type service, Type implementation, bool allowMultiple = false)
        {
            lock (_register)
            {
                if (implementation.GetConstructors().Length > 1)
                    throw new Exception($"Cannot bind {TypeHelper.Name(implementation)}, type has more than one constructor.");

                if (implementation.IsAbstract)
                    throw new Exception($"Cannot bind service type {TypeHelper.Name(implementation)}.");

                if (!allowMultiple && _register.Where(r => r.Service == service).Any())
                    throw new Exception($"Cannot bind service type {TypeHelper.Name(service)}, a binding for this already exists.");

                _register.Add(new Registration { Service = service, Implementation = implementation });
            }
        }

        /// <summary>
        /// Binds an instance to a service type. The given instance will always be returned for that service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="singleton"></param>
        public void RegisterSingleton<T>(object singleton)
        {
            RegisterSingleton(typeof(T), singleton);
        }

        /// <summary>
        /// Binds an instance to a service type. The given instance will always be returned for that service.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="singleton"></param>
        /// <exception cref="Exception"></exception>
        public void RegisterSingleton(Type service, object singleton)
        {
            lock (_register)
            { 
                if (_register.Where(r => r.Service == service).Any())
                    throw new Exception($"Cannot bind service type {TypeHelper.Name(service)}, a binding for this already exists.");

                _register.Add(new Registration { Service = service, Singleton = singleton });
            }
        }

        public T Resolve<T>() 
        {
            Type service = typeof(T);
            IEnumerable<Registration> matches = _register.Where(r => r.Service == service);
            if (matches.Count() > 1)
                throw new Exception($"Multiple implementations are registered for service {TypeHelper.Name(service)}.");

            if (!matches.Any())
                throw new Exception($"No implementations are registered for service {TypeHelper.Name(service)}.");

            Registration registration = matches.First();
            if (registration.Factory != null) 
            {
                ISimpleDIFactory factory = this.Resolve(registration.Factory) as ISimpleDIFactory;
                return (T)factory.Resolve<T>();
            }

            return (T)ResolveInternal(registration);
        }

        /// <summary>
        /// Creates an instance of an implementation that matches the given service type. Raises exception if multiple types are registered.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public object Resolve(Type service) 
        {
            IEnumerable<Registration> matches = _register.Where(r => r.Service == service);
            if (matches.Count() > 1)
                throw new Exception($"Multiple implementations are registered for service {TypeHelper.Name(service)}.");

            if (!matches.Any())
                throw new Exception($"No implementations are registered for service {TypeHelper.Name(service)}.");

            return ResolveInternal(matches.First());
        }

        public object ResolveImplementation(Type implementation) 
        {
            IEnumerable<Registration> matches = _register.Where(r => r.Implementation == implementation);
            if (matches.Count() > 1)
                throw new Exception($"Multiple implementations are registered for type {TypeHelper.Name(implementation)}.");

            if (!matches.Any())
                throw new Exception($"No implementations are registered for type {TypeHelper.Name(implementation)}.");

            return ResolveInternal(matches.First());
        }

        /// <summary>
        /// Resolves all implementations for a given service type.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public IEnumerable<object> ResolveAll(Type service)
        {
            IList<object> instances = new List<object>();
            IEnumerable<Registration> registrations = _register.Where(r => r.Service == service);
            if (!registrations.Any())
                throw new Exception($"No implementations registered for service {TypeHelper.Name(service)}.");

            foreach (Registration registration in registrations) 
                instances.Add(ResolveInternal(registration));

            return instances;
        }

        /// <summary>
        /// Resolves first implementation for a given service type.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public object ResolveFirst(Type service)
        {
            IList<object> instances = new List<object>();
            IEnumerable<Registration> registrations = _register.Where(r => r.Service == service);
            if (!registrations.Any())
                throw new Exception($"No implementations registered for service {TypeHelper.Name(service)}.");

            return ResolveInternal(registrations.First());
        }

        /// <summary>
        /// Creates an instance of the implementation for the given registration. If implementation has sub-dependencies, creates instances
        /// of those recursively.
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private object ResolveInternal(Registration registration)
        {
            if (registration.Singleton != null)
                return registration.Singleton;

            // safety null check
            if (registration.Implementation == null)
                throw new Exception("Implementation is null ; this should not happen");

            ConstructorInfo ctor = registration.Implementation.GetConstructors().First();
            CompiledConstructor compiledConstructor = null;

            if (!_constructors.TryGetValue(registration.Implementation, out compiledConstructor)) 
            {
                compiledConstructor = BuildConstructor(ctor);
                _constructors.Add(registration.Implementation, compiledConstructor);
            }

            IList<object> args = new List<object>();

            foreach (ParameterInfo parameterInfo in ctor.GetParameters())
            {
                if (!_register.Any(r => r.Service == parameterInfo.ParameterType))
                    throw new Exception($"Could not create instance of {TypeHelper.Name(registration.Implementation)}, ctor arg {TypeHelper.Name(parameterInfo.ParameterType)} is not registered");

                //  turtles all the way down
                object instance = Resolve(parameterInfo.ParameterType);
                args.Add(instance);
            }

            return compiledConstructor(args.ToArray());
        }

        /// <summary>
        /// Compiles a constructor based on the given constructorInfo. Compiled constructors are faster to instantation than those
        /// accessed directly with Reflection. Or at least, they were back in 2011, when this code was written.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ctor"></param>
        /// <returns></returns>
        private static CompiledConstructor BuildConstructor(ConstructorInfo ctor)
        {
            Type type = ctor.DeclaringType;
            ParameterInfo[] ctorParameters = ctor.GetParameters();

            //create a single param of type object[]
            ParameterExpression parameters = Expression.Parameter(typeof(object[]), "args");

            Expression[] argsExp = new Expression[ctorParameters.Length];

            //pick each arg from the params array 
            //and create a typed expression of them
            for (int i = 0; i < ctorParameters.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = ctorParameters[i].ParameterType;

                Expression paramAccessorExp = Expression.ArrayIndex(parameters, index);

                Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);

                argsExp[i] = paramCastExp;
            }

            //make a NewExpression that calls the
            //ctor with the args we just created
            NewExpression newExp = Expression.New(ctor, argsExp);

            //create a lambda with the New
            //Expression as body and our param object[] as arg
            LambdaExpression lambda = Expression.Lambda(typeof(CompiledConstructor), newExp, parameters);

            //compile it
            CompiledConstructor compiled = (CompiledConstructor)lambda.Compile();

            return compiled;
        }

        #endregion
    }
}
