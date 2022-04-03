using System;
using System.Text;
using System.Text.RegularExpressions;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using ReMod.Core.VRChat;
using ReModCE.AvatarPostProcess;
using ReModCE.Config;
using ReModCE.EvilEyeSDK;
using ReModCE.Loader;
using ReModCE.SDK;
using UnhollowerBaseLib;
using UnityEngine;
using VRC.Core;

namespace ReModCE.Core
{
    internal class AntiCrashUtils
    {
		private static readonly Regex duplicatedMeshNameRegex = new Regex("[a-zA-Z0-9]+(\\s|\\.\\d+)+(\\(\\d+\\)|\\d+|\\.\\d+)");

		private static readonly Regex numberRegex = new Regex("^\\d{5,20}");

		private static readonly Regex isNewPoyomiShader = new Regex("hidden\\/(locked\\/|)(\\.|)poiyomi\\/(\\s|•|★|\\?|)+poiyomi (pro|toon|cutout|transparent)(\\s|•|★|\\?|)+\\/[a-z0-9\\s\\.\\d-_\\!\\@\\#\\$\\%\\^\\&\\*\\(\\)=\\]\\[]+");

		private static readonly int maximumRenderQueue = 85899;

		private static readonly List<Material> antiCrashTempMaterialsList = new List<Material>();

		internal static void ProcessRenderer(Renderer renderer, ref AntiCrashRendererPostProcess previousProcess)
		{
			if (!Configuration.GetAvatarProtectionsConfig().AntiMeshCrash || !ProcessMeshPolygons(renderer, ref previousProcess.meshCount, ref previousProcess.nukedMeshes, ref previousProcess.polygonCount, ref previousProcess.removedBlendshapeKeys))
			{
				if (Configuration.GetAvatarProtectionsConfig().AntiMaterialCrash)
				{
					AntiCrashMaterialPostProcess antiCrashMaterialPostProcess = ProcessMaterials(renderer, previousProcess.nukedMaterials, previousProcess.materialCount);
					previousProcess.nukedMaterials = antiCrashMaterialPostProcess.nukedMaterials;
					previousProcess.materialCount = antiCrashMaterialPostProcess.materialCount;
				}
				if (Configuration.GetAvatarProtectionsConfig().AntiShaderCrash)
				{
					AntiCrashShaderPostProcess antiCrashShaderPostProcess = ProcessShaders(renderer, previousProcess.nukedShaders, previousProcess.shaderCount);
					previousProcess.nukedShaders = antiCrashShaderPostProcess.nukedShaders;
					previousProcess.shaderCount = antiCrashShaderPostProcess.shaderCount;
				}
			}
		}

		internal static bool ProcessMeshPolygons(Renderer renderer, ref int currentMeshes, ref int currentNukedMeshes, ref uint currentPolygonCount, ref bool removedBlendshapeKeys)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = renderer.TryCast<SkinnedMeshRenderer>();
			MeshFilter component = renderer.GetComponent<MeshFilter>();
			Mesh mesh = skinnedMeshRenderer?.sharedMesh ?? component?.sharedMesh;
			if (mesh == null)
			{
				return false;
			}
			if (currentMeshes >= Configuration.GetAvatarProtectionsConfig().MaxMeshes)
			{
				currentNukedMeshes++;
				UnityEngine.Object.DestroyImmediate(renderer.gameObject, allowDestroyingAssets: true);
				return true;
			}
			if (Configuration.GetAvatarProtectionsConfig().AntiBlendShapeCrash && skinnedMeshRenderer != null)
			{
				bool flag = false;
				bool flag2 = false;
				bool flag3 = false;
				bool flag4 = false;
				bool flag5 = false;
				for (int i = 0; i < mesh.blendShapeCount; i++)
				{
					string text = mesh.GetBlendShapeName(i).ToLower();
					if (text.Contains("reverted"))
					{
						flag = true;
					}
					if (text.Contains("posetorest"))
					{
						flag2 = true;
					}
					else if (text.Contains("key 22"))
					{
						flag3 = true;
					}
					else if (text.Contains("key 56"))
					{
						flag4 = true;
					}
					else if (text.Contains("slant"))
					{
						flag5 = true;
					}
				}
				if (flag && flag2 && flag3 && flag4 && flag5)
				{
					removedBlendshapeKeys = true;
					mesh.ClearBlendShapes();
				}
			}
			int num;
			try
			{
				num = mesh.subMeshCount;
			}
			catch (Exception)
			{
				num = 0;
			}
			try
			{
				renderer.GetSharedMaterials(antiCrashTempMaterialsList);
				int num2 = ProcessMesh(mesh, num, ref currentNukedMeshes, ref currentPolygonCount);
				if (num2 != -1)
				{
					antiCrashTempMaterialsList.RemoveRange(num2, antiCrashTempMaterialsList.Count - num2);
					renderer.SetMaterialArray((Il2CppReferenceArray<Material>)antiCrashTempMaterialsList.ToArray());
				}
				if (num + 2 < renderer.GetMaterialCount())
				{
					UnityEngine.Object.Destroy(renderer.gameObject);
					return true;
				}
			}
			catch (Exception e)
			{
				ReLogger.Error("ProcessMesh", e);
			}
			currentMeshes++;
			return false;
		}

		internal static AntiCrashMaterialPostProcess ProcessMaterials(Renderer renderer, int currentNukedMaterials, int currentMaterialCount)
		{
			int materialCount = renderer.GetMaterialCount();
			int num = currentMaterialCount + materialCount;
			if (num > Configuration.GetAvatarProtectionsConfig().MaxMaterials)
			{
				int num2 = ((currentMaterialCount < Configuration.GetAvatarProtectionsConfig().MaxMaterials) ? Configuration.GetAvatarProtectionsConfig().MaxMaterials : 0);
				int num3 = ((num2 == 0) ? materialCount : (num - Configuration.GetAvatarProtectionsConfig().MaxMaterials));
				if (num2 > materialCount)
				{
					num2 = materialCount;
				}
				int num4 = materialCount - num2;
				if (num3 > num4)
				{
					num3 = num4;
				}
				currentNukedMaterials += num3;
				num -= num3;
				if (materialCount == num3)
				{
					UnityEngine.Object.DestroyImmediate(renderer.gameObject, allowDestroyingAssets: true);
				}
				else
				{
					List<Material> list = new List<Material>();
					renderer.GetSharedMaterials(list);
					list.RemoveRange(num2, num3);
					renderer.materials = (Il2CppReferenceArray<Material>)list.ToArray();
				}
			}
			currentMaterialCount = num;
			return new AntiCrashMaterialPostProcess
			{
				nukedMaterials = currentNukedMaterials,
				materialCount = currentMaterialCount
			};
		}

		internal static AntiCrashShaderPostProcess ProcessShaders(Renderer renderer, int currentNukedShaders, int currentShaderCount)
		{
			if (renderer == null)
			{
				return new AntiCrashShaderPostProcess
				{
					nukedShaders = currentNukedShaders,
					shaderCount = currentShaderCount
				};
			}
			for (int i = 0; i < renderer.materials.Length; i++)
			{
				if (!(renderer.materials[i] == null))
				{
					currentShaderCount++;
					if (ProcessShader(renderer.materials[i]))
					{
						currentNukedShaders++;
					}
				}
			}
			return new AntiCrashShaderPostProcess
			{
				nukedShaders = currentNukedShaders,
				shaderCount = currentShaderCount
			};
		}

		internal static AntiCrashClothPostProcess ProcessCloth(Cloth cloth, AntiCrashClothPostProcess previousReport)
		{
			if (previousReport.clothCount >= Configuration.GetAvatarProtectionsConfig().MaxCloth)
			{
				previousReport.nukedCloths++;
				UnityEngine.Object.DestroyImmediate(cloth.gameObject, allowDestroyingAssets: true);
				return new AntiCrashClothPostProcess
				{
					nukedCloths = previousReport.nukedCloths,
					clothCount = previousReport.clothCount,
					currentVertexCount = previousReport.currentVertexCount
				};
			}
			Mesh mesh = cloth.GetComponent<SkinnedMeshRenderer>()?.sharedMesh;
			if (mesh == null)
			{
				previousReport.nukedCloths++;
				UnityEngine.Object.DestroyImmediate(cloth.gameObject, allowDestroyingAssets: true);
				return new AntiCrashClothPostProcess
				{
					nukedCloths = previousReport.nukedCloths,
					clothCount = previousReport.clothCount,
					currentVertexCount = previousReport.currentVertexCount
				};
			}
			int num = previousReport.currentVertexCount + mesh.vertexCount;
			if (num >= Configuration.GetAvatarProtectionsConfig().MaxClothVertices)
			{
				previousReport.nukedCloths++;
				UnityEngine.Object.DestroyImmediate(cloth.gameObject, allowDestroyingAssets: true);
				return new AntiCrashClothPostProcess
				{
					nukedCloths = previousReport.nukedCloths,
					clothCount = previousReport.clothCount,
					currentVertexCount = previousReport.currentVertexCount
				};
			}
			cloth.clothSolverFrequency = GeneralUtils.Clamp(cloth.clothSolverFrequency, 0f, Configuration.GetAvatarProtectionsConfig().MaxClothSolverFrequency);
			previousReport.currentVertexCount = num;
			previousReport.clothCount++;
			return new AntiCrashClothPostProcess
			{
				nukedCloths = previousReport.nukedCloths,
				clothCount = previousReport.clothCount,
				currentVertexCount = previousReport.currentVertexCount
			};
		}

		internal static void ProcessParticleSystem(ParticleSystem particleSystem, ref AntiCrashParticleSystemPostProcess post)
		{
			ParticleSystemRenderer component = particleSystem.GetComponent<ParticleSystemRenderer>();
			if (component == null)
			{
				post.nukedParticleSystems++;
				UnityEngine.Object.DestroyImmediate(particleSystem, allowDestroyingAssets: true);
				return;
			}
			particleSystem.main.ringBufferMode = ParticleSystemRingBufferMode.Disabled;
			particleSystem.main.simulationSpeed = GeneralUtils.Clamp(particleSystem.main.simulationSpeed, 0f, Configuration.GetAvatarProtectionsConfig().MaxParticleSimulationSpeed);
			particleSystem.collision.maxCollisionShapes = GeneralUtils.Clamp(particleSystem.collision.maxCollisionShapes, 0, Configuration.GetAvatarProtectionsConfig().MaxParticleCollisionShapes);
			particleSystem.trails.ribbonCount = GeneralUtils.Clamp(particleSystem.trails.ribbonCount, 0, Configuration.GetAvatarProtectionsConfig().MaxParticleTrails);
			particleSystem.emissionRate = GeneralUtils.Clamp(particleSystem.emissionRate, 0f, Configuration.GetAvatarProtectionsConfig().MaxParticleEmissionRate);
			for (int i = 0; i < particleSystem.emission.burstCount; i++)
			{
				ParticleSystem.Burst burst = particleSystem.emission.GetBurst(i);
				burst.maxCount = GeneralUtils.Clamp(burst.maxCount, (short)0, (short)Configuration.GetAvatarProtectionsConfig().MaxParticleEmissionBurstCount);
				burst.cycleCount = GeneralUtils.Clamp(burst.cycleCount, 0, Configuration.GetAvatarProtectionsConfig().MaxParticleEmissionBurstCount);
				particleSystem.emission.SetBurst(i, burst);
			}
			int num = Configuration.GetAvatarProtectionsConfig().MaxParticleLimit - post.currentParticleCount;
			if (num <= 0 && particleSystem.maxParticles > 100)
			{
				particleSystem.maxParticles = 100;
			}
			else if (particleSystem.maxParticles > num)
			{
				particleSystem.maxParticles = num;
			}
			if (component.renderMode == ParticleSystemRenderMode.Mesh)
			{
				Il2CppReferenceArray<Mesh> il2CppReferenceArray = new Il2CppReferenceArray<Mesh>(component.meshCount);
				component.GetMeshes(il2CppReferenceArray);
				uint currentPolygonCount = 0u;
				int currentNukedMeshes = 0;
				if (component.mesh != null && duplicatedMeshNameRegex.IsMatch(component.mesh.name))
				{
					component.enabled = false;
					particleSystem.playOnAwake = false;
					if (particleSystem.isPlaying)
					{
						particleSystem.Stop();
					}
				}
				foreach (Mesh item in il2CppReferenceArray)
				{
					int subMeshCount;
					try
					{
						subMeshCount = item.subMeshCount;
					}
					catch (Exception)
					{
						subMeshCount = 0;
					}
					component.GetSharedMaterials(antiCrashTempMaterialsList);
					int num2 = ProcessMesh(item, subMeshCount, ref currentNukedMeshes, ref currentPolygonCount);
					if (num2 != -1)
					{
						antiCrashTempMaterialsList.RemoveRange(num2, antiCrashTempMaterialsList.Count - num2);
					}
					List<Material>.Enumerator enumerator2 = antiCrashTempMaterialsList.GetEnumerator();
					while (enumerator2.MoveNext())
					{
						Material current2 = enumerator2.Current;
						if (!(current2 == null))
						{
							ProcessShader(current2);
						}
					}
					if (num2 != -1)
					{
						component.SetMaterialArray((Il2CppReferenceArray<Material>)antiCrashTempMaterialsList.ToArray());
					}
				}
				if (currentPolygonCount * particleSystem.maxParticles > Configuration.GetAvatarProtectionsConfig().MaxParticleMeshVertices)
				{
					int num4 = (particleSystem.maxParticles = (int)(Configuration.GetAvatarProtectionsConfig().MaxParticleMeshVertices / currentPolygonCount));
				}
			}
			if (particleSystem.maxParticles == 0)
			{
				post.nukedParticleSystems++;
				UnityEngine.Object.DestroyImmediate(particleSystem, allowDestroyingAssets: true);
			}
			post.currentParticleCount += particleSystem.maxParticles;
		}

		internal static int ProcessMesh(Mesh mesh, int subMeshCount, ref int currentNukedMeshes, ref uint currentPolygonCount)
		{
			int num = -1;
			for (int i = 0; i < subMeshCount; i++)
			{
				try
				{
					uint num2 = mesh.GetIndexCount(i);
					switch (mesh.GetTopology(i))
					{
						case MeshTopology.Triangles:
							num2 /= 3u;
							break;
						case MeshTopology.Quads:
							num2 /= 4u;
							break;
						case MeshTopology.Lines:
							num2 /= 2u;
							break;
					}
					if (currentPolygonCount + num2 > Configuration.GetAvatarProtectionsConfig().MaxPolygons)
					{
						currentPolygonCount += num2;
						currentNukedMeshes++;
						if (num == -1)
						{
							num = i;
						}
						UnityEngine.Object.DestroyImmediate(mesh, allowDestroyingAssets: true);
						break;
					}
					currentPolygonCount += num2;
					continue;
				}
				catch (Exception e)
				{
					ReLogger.Error("SubMesh Processor", e);
					continue;
				}
			}
			if (mesh == null)
			{
				return num;
			}
			if (GeneralUtils.IsBeyondLimit(mesh.bounds.extents, -100f, 100f))
			{
				UnityEngine.Object.DestroyImmediate(mesh, allowDestroyingAssets: true);
				return num;
			}
			if (GeneralUtils.IsBeyondLimit(mesh.bounds.size, -100f, 100f))
			{
				UnityEngine.Object.DestroyImmediate(mesh, allowDestroyingAssets: true);
				return num;
			}
			if (GeneralUtils.IsBeyondLimit(mesh.bounds.center, -100f, 100f))
			{
				UnityEngine.Object.DestroyImmediate(mesh, allowDestroyingAssets: true);
				return num;
			}
			if (GeneralUtils.IsBeyondLimit(mesh.bounds.min, -100f, 100f))
			{
				UnityEngine.Object.DestroyImmediate(mesh, allowDestroyingAssets: true);
				return num;
			}
			if (GeneralUtils.IsBeyondLimit(mesh.bounds.max, -100f, 100f))
			{
				UnityEngine.Object.DestroyImmediate(mesh, allowDestroyingAssets: true);
				return num;
			}
			return num;
		}

		internal static AntiCrashDynamicBonePostProcess ProcessDynamicBone(DynamicBone dynamicBone, int currentNukedDynamicBones, int currentDynamicBones)
		{
			if (currentDynamicBones >= Configuration.GetAvatarProtectionsConfig().MaxDynamicBones)
			{
				currentNukedDynamicBones++;
				UnityEngine.Object.DestroyImmediate(dynamicBone, allowDestroyingAssets: true);
				return new AntiCrashDynamicBonePostProcess
				{
					nukedDynamicBones = currentNukedDynamicBones,
					dynamicBoneCount = currentDynamicBones
				};
			}
			currentDynamicBones++;
			dynamicBone.m_UpdateRate = GeneralUtils.Clamp(dynamicBone.m_UpdateRate, 0f, 60f);
			dynamicBone.m_Radius = GeneralUtils.Clamp(dynamicBone.m_Radius, 0f, 2f);
			dynamicBone.m_EndLength = GeneralUtils.Clamp(dynamicBone.m_EndLength, 0f, 10f);
			dynamicBone.m_DistanceToObject = GeneralUtils.Clamp(dynamicBone.m_DistanceToObject, 0f, 1f);
			Vector3 endOffset = dynamicBone.m_EndOffset;
			endOffset.x = GeneralUtils.Clamp(endOffset.x, -5f, 5f);
			endOffset.y = GeneralUtils.Clamp(endOffset.y, -5f, 5f);
			endOffset.z = GeneralUtils.Clamp(endOffset.z, -5f, 5f);
			dynamicBone.m_EndOffset = endOffset;
			Vector3 gravity = dynamicBone.m_Gravity;
			gravity.x = GeneralUtils.Clamp(gravity.x, -5f, 5f);
			gravity.y = GeneralUtils.Clamp(gravity.y, -5f, 5f);
			gravity.z = GeneralUtils.Clamp(gravity.z, -5f, 5f);
			dynamicBone.m_Gravity = gravity;
			Vector3 force = dynamicBone.m_Force;
			force.x = GeneralUtils.Clamp(force.x, -5f, 5f);
			force.y = GeneralUtils.Clamp(force.y, -5f, 5f);
			force.z = GeneralUtils.Clamp(force.z, -5f, 5f);
			dynamicBone.m_Force = force;
			List<DynamicBoneCollider> list = new List<DynamicBoneCollider>();
			foreach (DynamicBoneCollider item in dynamicBone.m_Colliders.ToArray())
			{
				if (item != null && !list.Contains(item))
				{
					list.Add(item);
				}
			}
			dynamicBone.m_Colliders = list;
			return new AntiCrashDynamicBonePostProcess
			{
				nukedDynamicBones = currentNukedDynamicBones,
				dynamicBoneCount = currentDynamicBones
			};
		}

		internal static AntiCrashDynamicBoneColliderPostProcess ProcessDynamicBoneCollider(DynamicBoneCollider dynamicBoneCollider, int currentNukedDynamicBoneColliders, int currentDynamicBoneColliders)
		{
			if (currentDynamicBoneColliders >= Configuration.GetAvatarProtectionsConfig().MaxDynamicBoneColliders)
			{
				currentNukedDynamicBoneColliders++;
				UnityEngine.Object.DestroyImmediate(dynamicBoneCollider, allowDestroyingAssets: true);
				return new AntiCrashDynamicBoneColliderPostProcess
				{
					nukedDynamicBoneColliders = currentNukedDynamicBoneColliders,
					dynamicBoneColiderCount = currentDynamicBoneColliders
				};
			}
			currentDynamicBoneColliders++;
			dynamicBoneCollider.m_Radius = GeneralUtils.Clamp(dynamicBoneCollider.m_Radius, 0f, 50f);
			dynamicBoneCollider.m_Height = GeneralUtils.Clamp(dynamicBoneCollider.m_Height, 0f, 50f);
			Vector3 center = dynamicBoneCollider.m_Center;
			GeneralUtils.Clamp(center.x, -50f, 50f);
			GeneralUtils.Clamp(center.y, -50f, 50f);
			GeneralUtils.Clamp(center.z, -50f, 50f);
			dynamicBoneCollider.m_Center = center;
			return new AntiCrashDynamicBoneColliderPostProcess
			{
				nukedDynamicBoneColliders = currentNukedDynamicBoneColliders,
				dynamicBoneColiderCount = currentDynamicBoneColliders
			};
		}

		internal static AntiCrashLightSourcePostProcess ProcessLight(Light light, int currentNukedLights, int currentLights)
		{
			if (currentLights >= Configuration.GetAvatarProtectionsConfig().MaxLightSources)
			{
				currentNukedLights++;
				UnityEngine.Object.DestroyImmediate(light, allowDestroyingAssets: true);
			}
			currentLights++;
			return new AntiCrashLightSourcePostProcess
			{
				nukedLightSources = currentNukedLights,
				lightSourceCount = currentLights
			};
		}

		internal static bool ProcessTransform(Transform _, ref int currentTransforms)
		{
			if (currentTransforms >= Configuration.GetAvatarProtectionsConfig().MaxTransforms)
			{
				return true;
			}
			currentTransforms++;
			return false;
		}

		internal static bool ProcessConstraint(Behaviour constraint, ref int currentConstraints, ref int nukedConstraints)
		{
			if (currentConstraints >= Configuration.GetAvatarProtectionsConfig().MaxConstraints)
			{
				nukedConstraints++;
				UnityEngine.Object.DestroyImmediate(constraint.gameObject, allowDestroyingAssets: true);
				return true;
			}
			currentConstraints++;
			return false;
		}

		internal static bool ProcessRigidbody(Rigidbody rigidbody, ref int currentRigidbodies, ref int nukedRigidbodies)
		{
			if (currentRigidbodies >= Configuration.GetAvatarProtectionsConfig().MaxRigidbodies)
			{
				nukedRigidbodies++;
				UnityEngine.Object.DestroyImmediate(rigidbody.gameObject, allowDestroyingAssets: true);
				return true;
			}
			rigidbody.mass = GeneralUtils.Clamp(rigidbody.mass, 0f - Configuration.GetAvatarProtectionsConfig().MaxRigidbodyMass, Configuration.GetAvatarProtectionsConfig().MaxRigidbodyMass);
			rigidbody.maxAngularVelocity = GeneralUtils.Clamp(rigidbody.maxAngularVelocity, 0f - Configuration.GetAvatarProtectionsConfig().MaxRigidbodyAngularVelocity, Configuration.GetAvatarProtectionsConfig().MaxRigidbodyAngularVelocity);
			rigidbody.maxDepenetrationVelocity = GeneralUtils.Clamp(rigidbody.maxDepenetrationVelocity, 0f - Configuration.GetAvatarProtectionsConfig().MaxRigidbodyDepenetrationVelocity, Configuration.GetAvatarProtectionsConfig().MaxRigidbodyDepenetrationVelocity);
			return false;
		}

		internal static bool ProcessCollider(Collider collider, ref int currentColliders, ref int nukedColliders)
		{
			if (currentColliders >= Configuration.GetAvatarProtectionsConfig().MaxColliders)
			{
				nukedColliders++;
				UnityEngine.Object.DestroyImmediate(collider, allowDestroyingAssets: true);
				return true;
			}
			if ((collider.bounds.center.x < -100f && collider.bounds.center.x > 100f) || (collider.bounds.center.y < -100f && collider.bounds.center.y > 100f) || (collider.bounds.center.z < -100f && collider.bounds.center.z > 100f) || (collider.bounds.extents.x < -100f && collider.bounds.extents.x > 100f) || (collider.bounds.extents.y < -100f && collider.bounds.extents.y > 100f) || (collider.bounds.extents.z < -100f && collider.bounds.extents.z > 100f))
			{
				nukedColliders++;
				UnityEngine.Object.DestroyImmediate(collider, allowDestroyingAssets: true);
				return true;
			}
			currentColliders++;
			return false;
		}

		internal static bool ProcessJoint(Joint joint, ref int currentSpringJoints)
		{
			if (currentSpringJoints >= Configuration.GetAvatarProtectionsConfig().MaxSpringJoints)
			{
				UnityEngine.Object.DestroyImmediate(joint.gameObject, allowDestroyingAssets: true);
				return true;
			}
			currentSpringJoints++;
			joint.connectedMassScale = GeneralUtils.Clamp(joint.connectedMassScale, -25f, 25f);
			joint.massScale = GeneralUtils.Clamp(joint.massScale, -25f, 25f);
			joint.breakTorque = GeneralUtils.Clamp(joint.breakTorque, -100f, 100f);
			joint.breakForce = GeneralUtils.Clamp(joint.massScale, -100f, 100f);
			if (joint is SpringJoint springJoint)
			{
				springJoint.damper = GeneralUtils.Clamp(springJoint.damper, -100f, 100f);
				springJoint.maxDistance = GeneralUtils.Clamp(springJoint.maxDistance, -100f, 100f);
				springJoint.minDistance = GeneralUtils.Clamp(springJoint.minDistance, -100f, 100f);
				springJoint.spring = GeneralUtils.Clamp(springJoint.spring, -100f, 100f);
				springJoint.tolerance = GeneralUtils.Clamp(springJoint.tolerance, -100f, 100f);
			}
			return false;
		}

		internal static bool ProcessShader(Material material)
		{
			string text = material.shader.name.ToLower();
			if (!material.shader.isSupported)
			{
				SanitizeShader(material);
				return true;
			}
			if (ShaderUtils.IsFakeEngineShader(material))
			{
				SanitizeShader(material);
				return true;
			}
			if (isNewPoyomiShader.IsMatch(text))
			{
				return false;
			}
			if (ShaderUtils.blacklistedShaders.Contains(text))
			{
				SanitizeShader(material);
				return true;
			}
			int num = (Encoding.UTF8.GetByteCount(text) - text.Length) / 4;
			if (string.IsNullOrEmpty(text) || text.Length > 100 || material.shader.renderQueue > maximumRenderQueue || num > 10 || numberRegex.IsMatch(text))
			{
				SanitizeShader(material);
				return true;
			}
			return false;
		}

		internal static void SanitizeShader(Material material)
		{
			material.shader = ShaderUtils.GetStandardShader();
		}

		internal static void DisposeAvatar(GameObject avatar)
		{
			for (int i = 0; i < avatar.transform.childCount; i++)
			{
				UnityEngine.Object.DestroyImmediate(avatar.transform.GetChild(i).gameObject, allowDestroyingAssets: true);
			}
			Component[] array = avatar.GetComponents<Component>();
			for (int j = 0; j < array.Length; j++)
			{
				UnityEngine.Object.DestroyImmediate(array[j], allowDestroyingAssets: true);
			}
		}

		internal static void ProcessAvatarWhitelist(ApiAvatar avatar)
        {
            if (!Configuration.GetAvatarProtectionsConfig().WhitelistedAvatars.ContainsKey(avatar.id))
            {
                GeneralWrapper.AlertAction("Whitelist", "Are you sure you want to add " + avatar.name + " to your whitelist?", "Add", delegate
                {
                    GeneralWrapper.ClosePopup();
                    Configuration.GetAvatarProtectionsConfig().WhitelistedAvatars.Add(avatar.id, value: true);
                    Configuration.SaveAvatarProtectionsConfig();
                    GeneralUtils.RemoveAvatarFromCache(avatar.id);
                    PlayerExtensions.ReloadAllAvatars(PlayerWrapper.LocalVRCPlayer());
                }, "Cancel", delegate
                {
                    GeneralWrapper.ClosePopup();
                });
            }
            else if (Configuration.GetAvatarProtectionsConfig().WhitelistedAvatars[avatar.id])
            {
                GeneralWrapper.AlertAction("Whitelist", "Are you sure you want to remove " + avatar.name + " from your whitelist?", "Remove", delegate
                {
                    GeneralWrapper.ClosePopup();
                    Configuration.GetAvatarProtectionsConfig().WhitelistedAvatars.Remove(avatar.id);
                    Configuration.SaveAvatarProtectionsConfig();
                    GeneralUtils.RemoveAvatarFromCache(avatar.id);
					PlayerExtensions.ReloadAllAvatars(PlayerWrapper.LocalVRCPlayer());
				}, "Cancel", delegate
                {
                    GeneralWrapper.ClosePopup();
                });
            }
            else
            {
                GeneralWrapper.AlertAction("Whitelist", "Are you sure you want to add " + avatar.name + " to your whitelist?", "Add", delegate
                {
                    GeneralWrapper.ClosePopup();
                    Configuration.GetAvatarProtectionsConfig().WhitelistedAvatars[avatar.id] = true;
                    Configuration.SaveAvatarProtectionsConfig();
                    GeneralUtils.RemoveAvatarFromCache(avatar.id);
					PlayerExtensions.ReloadAllAvatars(PlayerWrapper.LocalVRCPlayer());
				}, "Cancel", delegate
                {
                    GeneralWrapper.ClosePopup();
                });
            }
        }
	}
}
