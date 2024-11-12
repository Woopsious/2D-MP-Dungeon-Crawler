using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ConeMesh : MonoBehaviour
{
	public int resolution;
	public float angle;
	public float radius;

	public Material material;
	CanvasRenderer canvasRenderer;

	private void Awake()
	{
		canvasRenderer = GetComponent<CanvasRenderer>();
		canvasRenderer.SetMaterial(material, null);
	}

	public void CreateConeMesh(float angle, float radius)
	{
		resolution = (int)(angle / 6); //scale resolution with cone size
		this.radius = radius;

		float angleStep = angle / resolution; //adjust angle
		float adjustedAngle = angle + angleStep;
		this.angle = adjustedAngle;

		canvasRenderer.SetMesh(GenerateConeMesh());
	}

	private Mesh GenerateConeMesh()
	{
		Mesh mesh = new();
		Vector3[] vertices = new Vector3[resolution + 1];
		int[] triangles = new int[resolution * 3];

		vertices[0] = Vector3.zero;
		float angleStep = angle / resolution;

		for (int i = 0; i < resolution; i++)
		{
			float angle = i * angleStep * Mathf.Deg2Rad;
			float x = Mathf.Cos(angle) * radius;
			float y = Mathf.Sin(angle) * radius;
			vertices[i + 1] = new Vector3(x, y, 0f);
		}

		for (int i = 0; i < resolution; i++)
		{
			int nextIndex = (i + 1) % resolution;
			triangles[i * 3] = 0;
			triangles[i * 3 + 1] = i + 1;
			triangles[i * 3 + 2] = nextIndex + 1;
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		return mesh;
	}
}