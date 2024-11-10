using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ConeMesh : MonoBehaviour
{
	public int angle;
	public float radius;

	private void Awake()
	{
		int resolution = angle / 6;
		float angleStep = (float)angle / resolution;
		float adjustedAngle = angle + angleStep;
		GetComponent<MeshFilter>().mesh = GeneratePartialCircleMesh(adjustedAngle, resolution);
	}

	private Mesh GeneratePartialCircleMesh(float adjustedAngle, int resolution)
	{
		Mesh mesh = new();
		Vector3[] vertices = new Vector3[resolution + 1];
		int[] triangles = new int[resolution * 3];

		vertices[0] = Vector3.zero;
		float angleStep = adjustedAngle / resolution;

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
	/*
	private Vector3[] verticies = new Vector3[5];
	private Vector2[] uv = new Vector2[5];
	private int[] triangles = new int[9];

	private Mesh mesh;

	private void Start()
	{
		GenerateMeshData();

		mesh = new Mesh();
		mesh.name = "Custom Mesh";

		gameObject.GetComponent<MeshFilter>().mesh = mesh;

		mesh.vertices = verticies;
		mesh.uv = uv;
		mesh.triangles = triangles;
	}
	private void GenerateMeshData()
	{
		verticies[0] = new Vector3(0,0);
		verticies[1] = new Vector3(0,1);
		verticies[2] = new Vector3(1,1);
		verticies[3] = new Vector3(1,0);
		verticies[4] = new Vector3(0,2);

		uv[0] = new Vector2(0, 0);
		uv[1] = new Vector2(0, 1);
		uv[2] = new Vector2(1, 1);
		uv[3] = new Vector2(1, 0);
		uv[4] = new Vector2(2, 0);

		triangles[0] = 0;
		triangles[1] = 1;
		triangles[2] = 2;

		triangles[3] = 0;
		triangles[4] = 2;
		triangles[5] = 3;

		triangles[6] = 1;
		triangles[7] = 2;
		triangles[8] = 4;
	}
	*/
}
