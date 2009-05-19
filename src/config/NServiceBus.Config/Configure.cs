﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus.ObjectBuilder;
using System.IO;
using System.Reflection;

namespace NServiceBus
{
    /// <summary>
    /// Central configuration entry point for NServiceBus.
    /// </summary>
    public class Configure
    {
        /// <summary>
        /// Provides static access to object builder functionality.
        /// </summary>
        public static IBuilder ObjectBuilder
        {
            get { return instance.Builder; }
        }
        
        /// <summary>
        /// Gets/sets the builder.
        /// Setting the builder should only be done by NServiceBus framework code.
        /// </summary>
        public IBuilder Builder { get; set; }

        /// <summary>
        /// Gets/sets the object used to configure components.
        /// This object should eventually reference the same container as the Builder.
        /// </summary>
        public IConfigureComponents Configurer { get; set; }

        /// <summary>
        /// Protected constructor to enable creation only via the With method.
        /// </summary>
        protected Configure() { }

        /// <summary>
        /// Creates a new configuration object scanning assemblies
        /// in the regular runtime directory.
        /// </summary>
        /// <returns></returns>
        public static Configure With()
        {
            if (instance == null)
                instance = new Configure();

            TypesToScan = new List<Type>(GetTypesInDirectory(AppDomain.CurrentDomain.BaseDirectory));

            return instance;
        }

        /// <summary>
        /// Configures nServiceBus to scan for assemblies 
        /// in the relevant web directory instead of regular
        /// runtime directory.
        /// </summary>
        /// <returns></returns>
        public static Configure WithWeb()
        {
            if (instance == null)
                instance = new Configure();

            TypesToScan = new List<Type>(GetTypesInDirectory(AppDomain.CurrentDomain.DynamicDirectory));

            return instance;
        }

        /// <summary>
        /// Configures nServiceBus to scan for assemblies
        /// in the given directory rather than the regular
        /// runtime directory.
        /// </summary>
        /// <param name="probeDirectory"></param>
        /// <returns></returns>
        public static Configure With(string probeDirectory)
        {
            if (instance == null)
                instance = new Configure();

            TypesToScan = new List<Type>(GetTypesInDirectory(probeDirectory));

            return instance;
        }

        /// <summary>
        /// Configures nServiceBus to scan the given assemblies only.
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static Configure With(params Assembly[] assemblies)
        {
            if (instance == null)
                instance = new Configure();

            var types = new List<Type>();
            new List<Assembly>(assemblies).ForEach((a) => { foreach (Type t in a.GetTypes()) types.Add(t); });

            TypesToScan = types;

            return instance;
        }

        /// <summary>
        /// Provides an instance to a startable bus.
        /// </summary>
        /// <returns></returns>
        public IStartableBus CreateBus()
        {
            return Builder.Build<IStartableBus>();
        }

        /// <summary>
        /// Returns types in assemblies found in the current directory.
        /// </summary>
        public static IEnumerable<Type> TypesToScan { get; private set; }

        private static IEnumerable<Type> GetTypesInDirectory(string path)
        {
            foreach (Type t in GetTypesInDirectoryWithExtension(path, "*.exe"))
                yield return t;
            foreach (Type t in GetTypesInDirectoryWithExtension(path, "*.dll"))
                yield return t;
        }

        private static IEnumerable<Type> GetTypesInDirectoryWithExtension(string path, string extension)
        {
            foreach (FileInfo file in new DirectoryInfo(path).GetFiles(extension, SearchOption.AllDirectories))
            {
                Assembly a = Assembly.LoadFrom(file.FullName);

                foreach (Type t in a.GetTypes())
                    yield return t;
            }
        }

        private static Configure instance;
    }
}
