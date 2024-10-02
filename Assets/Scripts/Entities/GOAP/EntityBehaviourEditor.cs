using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(EntityBehaviour))]
public class EntityBehaviourEditor : Editor
{
	public override void OnInspectorGUI()
	{
		EntityBehaviour entity = (EntityBehaviour)target;

		EditorGUILayout.Space();
		DrawDefaultInspector();

		EditorGUILayout.Space();

		if (entity.currentGoal != null)
		{
			EditorGUILayout.LabelField("Current Goal:", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(10);
			EditorGUILayout.LabelField(entity.currentGoal.Name);
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.Space();

		// Show current action
		if (entity.currentAction != null)
		{
			EditorGUILayout.LabelField("Current Action:", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(10);
			EditorGUILayout.LabelField(entity.currentAction.Name);
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.Space();

		// Show current plan
		if (entity.actionPlan != null)
		{
			EditorGUILayout.LabelField("Plan Stack:", EditorStyles.boldLabel);
			foreach (var a in entity.actionPlan.Actions)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(10);
				EditorGUILayout.LabelField(a.Name);
				EditorGUILayout.EndHorizontal();
			}
		}

		EditorGUILayout.Space();

		// Show beliefs
		EditorGUILayout.LabelField("Beliefs:", EditorStyles.boldLabel);
		if (entity.beliefs != null)
		{
			foreach (var belief in entity.beliefs)
			{
				if (belief.Key is "Nothing" or "Something") continue;
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(10);
				EditorGUILayout.LabelField(belief.Key + ": " + belief.Value.Evaluate());
				EditorGUILayout.EndHorizontal();
			}
		}

		EditorGUILayout.Space();
	}
}

[CustomEditor(typeof(BossEntityBehaviour))]
public class BossEntityBehaviourEditor : Editor
{
	public override void OnInspectorGUI()
	{
		BossEntityBehaviour entity = (BossEntityBehaviour)target;

		EditorGUILayout.Space();
		DrawDefaultInspector();

		EditorGUILayout.Space();

		if (entity.currentGoal != null)
		{
			EditorGUILayout.LabelField("Current Goal:", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(10);
			EditorGUILayout.LabelField(entity.currentGoal.Name);
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.Space();

		// Show current action
		if (entity.currentAction != null)
		{
			EditorGUILayout.LabelField("Current Action:", EditorStyles.boldLabel);
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(10);
			EditorGUILayout.LabelField(entity.currentAction.Name);
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.Space();

		// Show current plan
		if (entity.actionPlan != null)
		{
			EditorGUILayout.LabelField("Plan Stack:", EditorStyles.boldLabel);
			foreach (var a in entity.actionPlan.Actions)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(10);
				EditorGUILayout.LabelField(a.Name);
				EditorGUILayout.EndHorizontal();
			}
		}

		EditorGUILayout.Space();

		// Show beliefs
		EditorGUILayout.LabelField("Beliefs:", EditorStyles.boldLabel);
		if (entity.beliefs != null)
		{
			foreach (var belief in entity.beliefs)
			{
				if (belief.Key is "Nothing" or "Something") continue;
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(10);
				EditorGUILayout.LabelField(belief.Key + ": " + belief.Value.Evaluate());
				EditorGUILayout.EndHorizontal();
			}
		}

		EditorGUILayout.Space();
	}
}
