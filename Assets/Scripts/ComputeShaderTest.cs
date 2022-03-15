using UnityEngine;

public class ComputeShaderTest : MonoBehaviour
{
    public int size = 64;

    public ComputeShader startShader;
    public ComputeShader updateShader;

    private int mainFunction;
    private int diffuseFunction;

    private RenderTexture densityTexture;

    public MeshRenderer planeRenderer;
    private Material planeMat;

    private void Start()
    {
        planeMat = planeRenderer.sharedMaterial;
        mainFunction = updateShader.FindKernel("CSMain");
        diffuseFunction = updateShader.FindKernel("Diff");

        densityTexture = new RenderTexture(size, size, 24);
        densityTexture.enableRandomWrite = true;
        densityTexture.Create();
        planeMat.SetTexture("_Texture", densityTexture);


        updateShader.SetTexture(mainFunction, "DensityTex", densityTexture);
        updateShader.SetTexture(diffuseFunction, "DensityTex", densityTexture);
        updateShader.SetInt("N", size);


        updateShader.Dispatch(mainFunction, densityTexture.width / 8, densityTexture.height / 8, 1);
    }

    private void FixedUpdate()
    {
        // Diffuse
        updateShader.SetFloat("_DeltaTime", Time.deltaTime);

        for (int i = 0; i < 1; i++)
        {
            updateShader.Dispatch(diffuseFunction, densityTexture.width / 8, densityTexture.height / 8, 1);
        }
    }
}
