﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
#pragma warning disable CS0162

namespace ET
{
	public class CodeLoader: Singleton<CodeLoader>, ISingletonAwake
	{
		private Assembly assembly;
		
		public void Awake()
		{
		}

		public void Start()
		{
			if (!Define.EnableDll)
			{
				GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
				if (globalConfig.CodeMode != CodeMode.ClientServer)
				{
					throw new Exception("!ENABLE_CODES mode must use ClientServer code mode!");
				}
				
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				
				foreach (Assembly ass in assemblies)
				{
					string name = ass.GetName().Name;
					if (name == "Unity.Model")
					{
						this.assembly = ass;
					}
				}
				
				World.Instance.AddSingleton<CodeTypes, Assembly[]>(assemblies);
			}
			else
			{
				byte[] assBytes;
				byte[] pdbBytes;
				if (!Define.IsEditor)
				{
					Dictionary<string, UnityEngine.Object> dictionary = AssetsBundleHelper.LoadBundle("code.unity3d");
					assBytes = ((TextAsset)dictionary["Model.dll"]).bytes;
					pdbBytes = ((TextAsset)dictionary["Model.pdb"]).bytes;
					
					// 这里为了方便做测试，直接加载了Unity/Temp/Bin/Debug/Model.dll，真正打包要还原使用上面注释的代码
					//assBytes = File.ReadAllBytes(Path.Combine("../Unity", Define.BuildOutputDir, "Model.dll"));
					//pdbBytes = File.ReadAllBytes(Path.Combine("../Unity", Define.BuildOutputDir, "Model.pdb"));

					if (Define.EnableIL2CPP)
					{
						HybridCLRHelper.Load();
					}
				}
				else
				{
					assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Model.dll"));
					pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Model.pdb"));
				}
			
				this.assembly = Assembly.Load(assBytes, pdbBytes);

				Assembly hotfixAssembly = this.LoadHotfix();
				
				World.Instance.AddSingleton<CodeTypes, Assembly[]>(new []{typeof (World).Assembly, typeof(Init).Assembly, this.assembly, hotfixAssembly});
			}
			
			IStaticMethod start = new StaticMethod(this.assembly, "ET.Entry", "Start");
			start.Run();
		}

		private Assembly LoadHotfix()
		{
			byte[] assBytes;
			byte[] pdbBytes;
			if (!Define.IsEditor)
			{
				Dictionary<string, UnityEngine.Object> dictionary = AssetsBundleHelper.LoadBundle("code.unity3d");
				assBytes = ((TextAsset)dictionary["Hotfix.dll"]).bytes;
				pdbBytes = ((TextAsset)dictionary["Hotfix.pdb"]).bytes;
					
				// 这里为了方便做测试，直接加载了Unity/Temp/Bin/Debug/Hotfix.dll，真正打包要还原使用上面注释的代码
				//assBytes = File.ReadAllBytes(Path.Combine("../Unity", Define.BuildOutputDir, "Hotfix.dll"));
				//pdbBytes = File.ReadAllBytes(Path.Combine("../Unity", Define.BuildOutputDir, "Hotfix.pdb"));
			}
			else
			{
				assBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Hotfix.dll"));
				pdbBytes = File.ReadAllBytes(Path.Combine(Define.BuildOutputDir, "Hotfix.pdb"));
			}
			
			Assembly hotfixAssembly = Assembly.Load(assBytes, pdbBytes);

			return hotfixAssembly;
		}

		public void Reload()
		{
			Assembly hotfixAssembly = this.LoadHotfix();

			CodeTypes codeTypes = World.Instance.AddSingleton<CodeTypes, Assembly[]>(new []{typeof (World).Assembly, typeof(Init).Assembly, this.assembly, hotfixAssembly});
			codeTypes.CreateCode();

			Log.Debug($"reload dll finish!");
		}
	}
}