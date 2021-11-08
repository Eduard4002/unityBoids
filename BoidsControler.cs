using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BoidsControler : MonoBehaviour
{

    Boid[] boids;
    public int amountToSpawn;
    Dictionary<string,int> kernelIndices;
    public ComputeShader shader;
    ComputeBuffer boidsBuffer;
    RenderTexture texture;
    RenderTexture textureProcessed;
    [Range(0f,50f)]
    public float boundAmount;
    [Range(0.001f, 50f)]
    public float speedLimit;
    [Header("Seperation Variables:")]
    [Range(0.0001f,50)]
    public float avoidDistance;
    [Range(0.0001f,50)]
    public float avoidFactor = 0.05f;
    [Header("Cohesion Variables:")]
    [Range(0.0001f,50)]
    public float centeringFactor = 0.005f;
    [Range(0,300)]
    public float visualRange;
    [Header("Allignment Variables")]
    [Range(0.0001f,50f)]
    public float matchingFactor = 0.05f;

    [Header("Post process")]
    public float diffuseSpeed;
    public float evaporateSpeed;

    // Start is called before the first frame update
    void Start()
    {
        kernelIndices = new Dictionary<string, int>();
        kernelIndices.Add("Update", shader.FindKernel("Update"));
        kernelIndices.Add("Postprocess", shader.FindKernel("Postprocess"));

        createNewTexture(ref texture);
        
        shader.SetFloat("width",Screen.width);
        shader.SetFloat("height",Screen.height);
        shader.SetFloat("fixedDeltaTime",Time.fixedDeltaTime);
        boids = new Boid[amountToSpawn];

        InitPositions();
    }
    void FixedUpdate(){
        shader.SetTexture(kernelIndices["Update"],"Result",texture);
        shader.SetInt("numBoids", amountToSpawn);
        shader.SetFloat("centeringFactor",centeringFactor);
        shader.SetFloat("avoidDistance",avoidDistance);
        shader.SetFloat("avoidFactor",avoidFactor);
        shader.SetFloat("matchingFactor",matchingFactor);
        shader.SetFloat("visualRange",visualRange);
        shader.SetFloat("boundAmount",boundAmount);
        shader.SetFloat("speed",speedLimit);
        shader.SetBuffer(kernelIndices["Update"],"boids", boidsBuffer);
        shader.Dispatch(kernelIndices["Update"],texture.width / 16, 1,1);        

        createNewTexture(ref textureProcessed);

        shader.SetFloat("diffuseSpeed",diffuseSpeed);
        shader.SetFloat("evaporateSpeed",evaporateSpeed);
        shader.SetTexture(kernelIndices["Postprocess"], "Result", texture);
        shader.SetTexture(kernelIndices["Postprocess"], "ResultProcessed", textureProcessed);
        shader.Dispatch(kernelIndices["Postprocess"], Screen.width / 8, Screen.height / 8, 1);

        texture.Release();
        texture = textureProcessed;
    }

    void InitPositions(){
        for(int i = 0; i < amountToSpawn;i++){
            boids[i].position = new Vector2(Random.Range(0,Screen.width), Random.Range(0,Screen.height));
        }
        boidsBuffer = new ComputeBuffer(boids.Length,sizeof(float) * 9);
        boidsBuffer.SetData(boids);
    }

    void createNewTexture(ref RenderTexture renderTexture){
        renderTexture = new RenderTexture(Screen.width,Screen.height,0);
        renderTexture.enableRandomWrite = true;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.Create();
    }
    
    void OnRenderImage(RenderTexture source, RenderTexture destination){
        Graphics.Blit(textureProcessed,destination);
    }
    void OnDestroy(){
        texture.Release();
        textureProcessed.Release();
        boidsBuffer.Release();
    }
    struct Boid{
        public Vector2 position;
        public Vector2 deltaPosition;
        public float index;
        public Vector4 color;
   
    }
}
