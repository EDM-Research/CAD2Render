
using System.Collections.Generic;
using UnityEngine;


public class ViewTrajectory : RandomizerInterface
{
    /* warehouseScene
    private List<Vector3> scanPoses = new List<Vector3> {
        new Vector3(33.207f, 1.45f, 53.01f),
        new Vector3(29.511f, 1.45f, 46.02f),
        new Vector3(33.07f, 1.45f, 39.73f),
        new Vector3(33.07f, 1.45f, 31.19f),
        new Vector3(29.627f, 1.45f, 26.218f),
        new Vector3(32.88f, 1.45f, 20.7f),
        new Vector3(30.98f, 1.45f, 11.87f),
        new Vector3(24.882f, 1.45f, 7.399f),
        new Vector3(19.83f, 1.45f, 11.934f),
        new Vector3(14.142f, 1.45f, 7.81f),
        new Vector3(9.204f, 1.45f, 12.32f),
        new Vector3(21.33f, 1.45f, 22.99f),
        new Vector3(21.28f, 1.45f, 31.44f),
        new Vector3(27.47f, 1.45f, 32.959f),
        new Vector3(21.28f, 1.45f, 38.8f),
        new Vector3(21.54f, 1.45f, 47.52f),
        new Vector3(21.54f, 1.45f, 54.865f),
        new Vector3(15.409f, 1.45f, 56.836f),
        new Vector3(15.409f, 1.45f, 48.4f),
        new Vector3(15.409f, 1.45f, 38.9f),
        new Vector3(15.409f, 1.45f, 32.67f),
        new Vector3(15.409f, 1.45f, 25.27f),
        new Vector3(15.409f, 1.45f, 17.782f),
        new Vector3(9.204f, 1.45f, 18.69f),
        new Vector3(9.204f, 1.45f, 26.09f),
        new Vector3(9.204f, 1.45f, 32.928f),
        new Vector3(9.204f, 1.45f, 39.482f),
        new Vector3(9.204f, 1.45f, 46.85f),
        new Vector3(9.405f, 1.45f, 53.916f),
    };/*/// appratment scene
    private List<Vector3> scanPoses = new List<Vector3> {
        new Vector3(22.52087f, 13.90853f, 2.897617f),
        new Vector3(25.39687f, 13.90853f, -1.176383f),
        new Vector3(21.07087f, 13.90853f, -2.462383f),
        new Vector3(25.40887f, 13.90853f, -5.975383f),
        new Vector3(31.07887f, 13.90853f, -1.331383f),
    };//*/

    private Camera _camera;
	private Camera mainCamera
	{
		get
		{
			if (!_camera)
			{
				_camera = Camera.main;
			}
			return _camera;
		}
	}


	public override ScriptableObject getDataset()
    {
        return null;
    }

    int i = 0;
    public override void Randomize(ref RandomNumberGenerator rng, SceneIteratorInterface sceneIterator = null)
    {
        mainCamera.transform.position = scanPoses[i%scanPoses.Count];
        i++;
    }
}