using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

//[ExecuteInEditMode]
[AddComponentMenu("Image Effects/GlitchEffect")]
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(VideoPlayer))]
public class VHSPostProcessEffect : MonoBehaviour
{
	public Shader shader;
	public VideoClip VHSClip;

	private float _yScanline;
	private float _xScanline;
	private Material _material = null;
	private VideoPlayer _player;

	void Start()
	{
		Initialize();
	}

	public void Initialize()
    {
		_material = new Material(shader);
		_player = GetComponent<VideoPlayer>();
		_player.isLooping = true;
		_player.renderMode = VideoRenderMode.CameraNearPlane;
		_player.audioOutputMode = VideoAudioOutputMode.None;
		_player.url = Application.streamingAssetsPath + "/" + "glitch.mp4";
		StartCoroutine(DoubleCheck());
	}

	IEnumerator DoubleCheck()
    {
		_player.Play();
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		_player.Play();
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		_material.SetTexture("_VHSTex", _player.texture);

		_yScanline += Time.deltaTime * 0.01f;
		_xScanline -= Time.deltaTime * 0.1f;

		if (_yScanline >= 1)
		{
			_yScanline = Random.value;
		}
		if (_xScanline <= 0 || Random.value < 0.05)
		{
			_xScanline = Random.value;
		}
		_material.SetFloat("_yScanline", _yScanline);
		_material.SetFloat("_xScanline", _xScanline);
		Graphics.Blit(source, destination, _material);
	}

	protected void OnDisable()
	{
		if (_material)
		{
			DestroyImmediate(_material);
		}
	}

    private void Update()
    {

	}
}
