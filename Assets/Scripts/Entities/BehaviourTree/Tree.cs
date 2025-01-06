using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class Tree : NetworkBehaviour
{

	private BTNode _root = null;

	protected virtual void Start()
	{
		_root = SetupTree();
	}

	protected virtual void Update()
	{
		if (_root != null)
			_root.Evaluate();
	}

	protected abstract BTNode SetupTree();

}
