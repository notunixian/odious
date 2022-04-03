using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace ReModCE.Core
{
    internal class ShaderUtils
    {
		private static readonly List<string> engineShaders = new List<string> { "shader", "diffuse", "particle", "transparent/diffuse", "unlit/texture" };

		internal static List<string> blacklistedShaders = new List<string>
		{
			"pretty", "bluescreen", "tesselation", "tesselated", "crasher", "instant crash paralyzer", "worldkill", "tessellation", "tessellated", "oofer",
			"xxx", "dbtc", "kyuzu", "distancebased", "waifuserp", "loops", "diebitch", "thotdestroyer", "niggathishurts", "izzyrfc",
			"izzyfrag", "izzycdf", "flat lite toon", "shadowschoolbully", "school shooter", "okbuddy", "basically private", "no sharing please", "hdjegbhuaj276312378632785678324547354t2egdy", "custom/oof",
			"custom/lag", "custom/killer", "custom/kyscotty", "kysautismoñoño", ".star/bacon", "god's children/holy water shader", "stonapse/worldkill", "stonapse/worldhit", "worldkill", "9874654876468d46as5d46as5d7a98we7a84d65as468w7e9q6as8d6s4d5as4d5as6da4s6546a8ew76a8w4e687eq89e7",
			"lorplefoncondominiumlistile14141414", "be careful", "7345687/6482375wer", "swisscheeseistasty99999999999/swisscheeseistasty99999999999", "undertaker/will ban you", "g\u033d\u0353l\u033d\u0353o\u033d\u0353m\u033d\u0353e\u033d\u0353e\u033d\u0353/", "yikersdudeg\u030c\u0342\u0312o\u0313\u0367\u0305o\u0346\u0346\u0303d\u034b\u036a\u030c \u0303\u0368\u0309l\u031a\u0300\u0314u\u0305\u0305\u033dc\u033e\u033e\u034ak\u0357\u0364\u034c \u0309\u0352\u0314s\u0307\u0357\u033dt\u0314\u034b\u036ce\u0363\u036a\u0368a\u030a\u030a\u0367l\u0308\u0305\u0306\u0489\u0361\u0323\u0330i\u0366\u0314\u035bn\u036c\u0308\u0364g\u0302\u0368\u034b \u0344\u0312\u030dt\u0368\u0363\u0307h\u034b\u036d\u030bi\u0312\u033d\u0311s\u0306\u0366\u036b \u036b\u036e\u0368\u0489\u032b\u0348\u0320b\u0357\u0369\u0363u\u0310\u036e\u030fd\u0343\u0301\u0313d\u0302\u030e\u0307y\u0350\u036c\u0357", "卍アイアンウィルが最高卍", "randomname     1eg4ww", "custo5455245m",
			"565416541651", "üõõüõaseio1", "duäöü", "ggo login body", "u go home", "no this is mine now", "poiyomi/imeatingmymic", "instant yeeet", "niggie", "ebola",
			"undetected", "got em", "retard", "retrd", "11h2hh3hjej3", "????????0????/???????/????", "standard on cylnder", "almgp/nuke", "hello <3", "c4",
			"Particle", "ATENÇAO", "grims_world_clap", "Slipknot/grim/Planet crash", "Planet crash"
		};

		private static Shader standardShader;

		private static Shader diffuseShader;

		internal static Shader GetStandardShader()
		{
			if (standardShader == null)
			{
				standardShader = Shader.Find("Standard");
			}
			return standardShader;
		}

		internal static Shader GetDiffuseShader()
		{
			if (diffuseShader == null)
			{
				diffuseShader = Shader.Find("Diffuse");
			}
			return diffuseShader;
		}

		internal static bool IsFakeEngineShader(Material material)
		{
			for (int i = 0; i < engineShaders.Count; i++)
			{
				if (material.shader.name == engineShaders[i] && material.shaderKeywords.Length == 0)
				{
					return true;
				}
			}
			return false;
		}
	}
}
