using UnityEngine;

public class ComputeShaderTest : MonoBehaviour
{
    public ComputeShader startShader;
    public ComputeShader updateShader;
    private RenderTexture renderTexture;

    public MeshRenderer planeRenderer;
    private Material planeMat;

    private void Start()
    {
        planeMat = planeRenderer.sharedMaterial;

        renderTexture = new RenderTexture(32, 32, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        planeMat.SetTexture("_Texture", renderTexture);

        startShader.SetTexture(0, "Result", renderTexture);
        startShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);
    }

    private void Update()
    {
        updateShader.SetFloat("_DeltaTime", Time.deltaTime);
        updateShader.SetTexture(0, "Result", renderTexture);
        updateShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);
    }
}
