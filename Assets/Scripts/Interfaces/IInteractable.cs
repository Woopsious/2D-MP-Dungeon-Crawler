using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
	void Interact(PlayerController playerController);

	void UnInteract(PlayerController playerController);
}
