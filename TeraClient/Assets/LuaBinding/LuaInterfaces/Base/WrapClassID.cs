using System;

public class WrapClassID
{
	private static Type[] _WrapList = new Type[61];
	public static void Register()
	{
		_WrapList[0] = typeof(UnityEngine.Component);
		_WrapList[1] = typeof(UnityEngine.Behaviour);
		_WrapList[2] = typeof(UnityEngine.MonoBehaviour);
		_WrapList[3] = typeof(UnityEngine.EventSystems.UIBehaviour);
		_WrapList[4] = typeof(UnityEngine.UI.Selectable);
		_WrapList[5] = typeof(UnityEngine.SystemInfo);
		_WrapList[6] = typeof(UnityEngine.AnimationClip);
		_WrapList[7] = typeof(UnityEngine.Time);
		_WrapList[8] = typeof(UnityEngine.Resources);
		_WrapList[9] = typeof(UnityEngine.Light);
		_WrapList[10] = typeof(UnityEngine.RectTransform);
		_WrapList[11] = typeof(UnityEngine.Canvas);
		_WrapList[12] = typeof(UnityEngine.UI.Button);
		_WrapList[13] = typeof(UnityEngine.UI.GraphicRaycaster);
		_WrapList[14] = typeof(UnityEngine.UI.Text);
		_WrapList[15] = typeof(UnityEngine.UI.Slider);
		_WrapList[16] = typeof(UnityEngine.UI.Image);
		_WrapList[17] = typeof(UnityEngine.UI.InputField);
		_WrapList[18] = typeof(UnityEngine.UI.Toggle);
		_WrapList[19] = typeof(UnityEngine.UI.Scrollbar);
		_WrapList[20] = typeof(DG.Tweening.DOTweenAnimation);
		_WrapList[21] = typeof(DG.Tweening.DOTweenPlayer);
		_WrapList[22] = typeof(CGameSession);
		_WrapList[23] = typeof(CFxOne);
		_WrapList[24] = typeof(CMotor);
		_WrapList[25] = typeof(CLinearMotor);
		_WrapList[26] = typeof(CTargetMotor);
		_WrapList[27] = typeof(CBallCurvMotor);
		_WrapList[28] = typeof(CParabolicMotor);
		_WrapList[29] = typeof(CMutantBezierMotor);
		_WrapList[30] = typeof(CHUDFollowTarget);
		_WrapList[31] = typeof(GameObjectPool);
		_WrapList[32] = typeof(EntityComponent.AnimationUnit);
		_WrapList[33] = typeof(EntityComponent.CombatStateChangeBehaviour);
		_WrapList[34] = typeof(EntityComponent.NpcStandBehaviour);
		_WrapList[35] = typeof(EntityComponent.HorseStandBehaviour);
		_WrapList[36] = typeof(CFxFollowTarget);
		_WrapList[37] = typeof(CGhostEffectMan);
		_WrapList[38] = typeof(UIEventListener);
		_WrapList[39] = typeof(GBase);
		_WrapList[40] = typeof(GNewUIBase);
		_WrapList[41] = typeof(GNewGridBase);
		_WrapList[42] = typeof(GNewListBase);
		_WrapList[43] = typeof(GNewList);
		_WrapList[44] = typeof(GNewListLoop);
		_WrapList[45] = typeof(GBlood);
		_WrapList[46] = typeof(GText);
		_WrapList[47] = typeof(UITemplate);
		_WrapList[48] = typeof(GNewTableBase);
		_WrapList[49] = typeof(GNewTabList);
		_WrapList[50] = typeof(GNewLayoutTable);
		_WrapList[51] = typeof(GButton);
		_WrapList[52] = typeof(GScaleScroll);
		_WrapList[53] = typeof(GNewUI.GUIScene);
		_WrapList[54] = typeof(GNewIOSToggle);
		_WrapList[55] = typeof(GDragablePageView);
		_WrapList[56] = typeof(UnityEngine.Animation);
		_WrapList[57] = typeof(GImageModel);
		_WrapList[58] = typeof(UnityEngine.Camera);
		_WrapList[59] = typeof(GWebView);
	}
	public static Type GetClassType(int id)
	{
		if (id < 0 || id >= _WrapList.Length)
			return null;
		return _WrapList[id];
	}
}
