using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Threading.Tasks;
using TMPro;

public class Map : MonoBehaviour
{
    // Start is called before the first frame update

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public ProgressBarre progressBarre;
    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

    }   

public IEnumerator generateMesh(  GeneralStatUtils _gen )
{
    if(progressBarre.ProcessingCheck())
    {
        yield break;
    }


    progressBarre.setAction("Génération du mesh");

    int width = _gen.it_data.size.x;
    int height = _gen.it_data.size.y;


    progressBarre.start((uint)height);


    float topLeftX = ( width / _gen.it_data.reso) / -2f;
    float topLeftZ = (height /  _gen.it_data.reso) / 2f;


    MeshData meshData = new MeshData(width, height);

    int vertexIndex = 0;


    // Génération des sommets et des UVs
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float h = (float)_gen.it_data.data[x, y];

                if (y == (int)_gen.limite.getLimiteYMin((uint)(x)) - 1)
                    h = 0;

                    
                float xReso = (float)x / (float)_gen.it_data.reso;
                float yReso = (float)y / (float)_gen.it_data.reso;

                // Gérer les valeurs NaN
                if (float.IsNaN(h)) h = 0;

                // Ajouter le sommet
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + xReso, h, -topLeftZ + yReso);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)(width ) + (0.5f/(float)(width )), y / (float)(height )+ (0.5f/(float)(width )));

                // Ajout des triangles
                if ( x < width-1 && y < height && y <= (int)_gen.limite.getLimiteYMax( (uint)(x) ) && y >=(int)_gen.limite.getLimiteYMin( (uint)(x) )-1)
                {
                    int a = vertexIndex;
                    int b = vertexIndex + 1;
                    int c = vertexIndex + width;
                    int d = vertexIndex + width + 1;
                    
                        if (d < meshData.vertices.Length)
                        {
                            meshData.addTriangle(a, c, b); // Triangle 1 // acb
                            meshData.addTriangle(b, c, d); // Triangle 2 // bcd
                        }
                    
                }

                vertexIndex++;
            }
            if(progressBarre.validUpdate((uint)y))
            {
                yield return new WaitForSeconds(0.01f);
            }
            
        }
    meshFilter.sharedMesh = meshData.createMesh();
    meshRenderer.sharedMaterial.mainTexture = _gen.map2d;

    progressBarre.stop();
    progressBarre.setAction("Mesh généré");
}



    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    
    public int triangleIndex = 0;
    public MeshData(int width , int height)
    {
        vertices = new Vector3[width * height];
        uvs = new Vector2[width * height];
        triangles = new int[(width-1) * (height-1) * 6];
    }

    public void addTriangle( int a , int b , int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex+1] = b;
        triangles[triangleIndex+2] = c;

        triangleIndex += 3;
    }

    public Mesh createMesh()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }
}
