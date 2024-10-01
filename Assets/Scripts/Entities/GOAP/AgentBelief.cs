using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BeliefFactory
{
	readonly EntityStats entity;
	readonly Dictionary<string, AgentBelief> beliefs;

	public BeliefFactory(EntityStats entity, Dictionary<string, AgentBelief> beliefs)
	{
		this.entity = entity;
		this.beliefs = beliefs;
	}

	public void AddBelief(string key, Func<bool> condition)
	{
		beliefs.Add(key, new AgentBelief.Builder(key)
			.WithCondition(condition)
			.Build());
	}

	public void AddSensorBelief(string key, EntityBehaviour entity, PlayerController playerTarget)
	{
		beliefs.Add(key, new AgentBelief.Builder(key)
			.WithCondition(() => entity.CurrentPlayerTargetVisible())
			.Build());
	}

	public void AddLocationBelief(string key, float distance, Transform locationCondition)
	{
		AddLocationBelief(key, distance, locationCondition.position);
	}

	public void AddLocationBelief(string key, float distance, Vector3 locationCondition)
	{
		beliefs.Add(key, new AgentBelief.Builder(key)
			.WithCondition(() => InRangeOf(locationCondition, distance))
			.WithLocation(() => locationCondition)
			.Build());
	}

	bool InRangeOf(Vector3 pos, float range) => Vector3.Distance(entity.transform.position, pos) < range;
}

public class AgentBelief
{
	public string Name { get; }

	Func<bool> condition = () => false;
	Func<Vector3> observedLocation = () => Vector3.zero;

	public Vector3 Location => observedLocation();

	AgentBelief(string name)
	{
		Name = name;
	}

	public bool Evaluate() => condition();

	//belif constructor
	public class Builder
	{
		readonly AgentBelief belief;

		public Builder(string name)
		{
			belief = new AgentBelief(name);
		}

		public Builder WithCondition(Func<bool> condition)
		{
			belief.condition = condition;
			return this;
		}

		public Builder WithLocation(Func<Vector3> observedLocation)
		{
			belief.observedLocation = observedLocation;
			return this;
		}

		public AgentBelief Build()
		{
			return belief;
		}
	}
}
