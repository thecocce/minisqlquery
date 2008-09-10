﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Castle.Windsor.Configuration;
using Castle.Core;

namespace MiniSqlQuery.Core
{
	/// <summary>
	/// The core services of the application (singleton).
	/// </summary>
	[Singleton]
	public class ApplicationServices : IApplicationServices
	{
		private static readonly IWindsorContainer _container;

		private Dictionary<Type, IPlugIn> _plugins = new Dictionary<Type,IPlugIn>();

		static ApplicationServices()
		{
			IConfigurationInterpreter config = new XmlInterpreter("Configuration.xml"); 
			_container = new WindsorContainer(config);
		}

		/// <summary>
		/// A reference to the singleton instance of the services for this application.
		/// </summary>
		/// <value>The singleton instance of <see cref="IApplicationServices"/>.</value>
		public static IApplicationServices Instance
		{
			get
			{
				return (IApplicationServices)_container.GetService(typeof(IApplicationServices));
			}
		}

		/// <summary>
		/// The Dependency Injection container.
		/// </summary>
		public IWindsorContainer Container
		{
			get
			{
				return _container;
			}
		}

		/// <summary>
		/// The application host window.
		/// </summary>
		/// <value>The host window - a <see cref="Form"/>.</value>
		public IHostWindow HostWindow
		{
			get
			{
				return (IHostWindow)_container.GetService(typeof(IHostWindow));
			}
		}

		/// <summary>
		/// The application settings instance.
		/// </summary>
		/// <value>A reference to the settings handler.</value>
		public IApplicationSettings Settings
		{
			get
			{
				return (IApplicationSettings)_container.GetService(typeof(IApplicationSettings));
			}
		}

		/// <summary>
		/// A dictionary of the current plugins for this application.
		/// </summary>
		/// <value>A reference to the plugin dictionary.</value>
		public Dictionary<Type, IPlugIn> Plugins
		{
			get
			{
				return _plugins;
			}
		}

		/// <summary>
		/// Loads the <paramref name="plugIn"/> (calling its <see cref="IPlugIn.LoadPlugIn"/> method) and
		/// adds it to the <see cref="Plugins"/> dictionary for access by other services.
		/// </summary>
		/// <param name="plugIn">The plugin to load.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="plugIn"/> is null.</exception>
		public void LoadPlugIn(IPlugIn plugIn)
		{
			if (plugIn == null)
			{
				throw new ArgumentNullException("plugIn");
			}

			plugIn.LoadPlugIn(this);
			_plugins.Add(plugIn.GetType(), plugIn);
			_container.AddComponent(plugIn.PluginName, typeof(IPlugIn), plugIn.GetType());
		}

		/// <summary>
		/// Loads a plugin configured via the <see cref="Container"/>.
		/// </summary>
		/// <param name="plugInKeyName">The "key" of the service.</param>
		public void LoadPlugFromContainer(string plugInKeyName)
		{
			if (plugInKeyName == null)
			{
				throw new ArgumentNullException("plugInKeyName");
			}

			IPlugIn plugin = Container[plugInKeyName] as IPlugIn;
			LoadPlugIn(plugin);
		}

		/// <summary>
		/// Initializes the plugins that have been loaded during application startup.
		/// </summary>
		public void InitializePlugIns()
		{
			foreach (IPlugIn plugIn in _plugins.Values)
			{
				try
				{
					if (HostWindow != null)
					{
						HostWindow.SetStatus(null, "Initializing " + plugIn.PluginName);
					}
					plugIn.InitializePlugIn();
					//err?
				}
				catch (Exception exp)
				{
					if (HostWindow == null)
					{
						throw exp;
					}
					else
					{
						HostWindow.DisplayMessageBox(
							null, 
							"Error Initializing " + plugIn.PluginName,
							"Plugin Error",
							MessageBoxButtons.OK,
							MessageBoxIcon.Warning,
							MessageBoxDefaultButton.Button1, 
							0,
							null,
							null);
					}
				}
			}
		}

	}
}