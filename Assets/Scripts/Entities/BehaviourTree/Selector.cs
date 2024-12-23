using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : BTNode
{
	public Selector() : base() { }
	public Selector(List<BTNode> children) : base(children) { }

	public override NodeState Evaluate()
	{
		foreach (BTNode node in children)
		{
			switch (node.Evaluate())
			{
				case NodeState.FAILURE:
				continue;
				case NodeState.SUCCESS:
				state = NodeState.SUCCESS;
				return state;
				case NodeState.RUNNING:
				state = NodeState.RUNNING;
				return state;
				default:
				continue;
			}
		}

		state = NodeState.FAILURE;
		return state;
	}

}
