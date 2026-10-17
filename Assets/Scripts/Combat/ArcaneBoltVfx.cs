using UnityEngine;

/// <summary>Cosmetic only. Projectile owns movement, hit tests and damage; effects use scaled gameplay time.</summary>
public sealed class ArcaneBoltVfx : MonoBehaviour
{
    [SerializeField] private Transform outerArc;
    [SerializeField] private Transform innerArc;
    [SerializeField] private ParticleSystem trail;
    [SerializeField] private ParticleSystem impact;
    [SerializeField] private Renderer orbBody;
    private MaterialPropertyBlock properties;
    private float phase;
    public float Phase=>phase;
    private bool finished;

    private void Awake()
    {
        phase=Mathf.Abs(GetInstanceID()*0.1234f)%6.28f;
        properties=new MaterialPropertyBlock();
    }

    private void Update()
    {
        phase+=Time.deltaTime;
        if(orbBody!=null)
        {
            properties.SetFloat("_Phase",phase);
            orbBody.SetPropertyBlock(properties);
        }
        if(outerArc!=null) outerArc.Rotate(0,0,240*Time.deltaTime);
        if(innerArc!=null) innerArc.Rotate(0,0,-320*Time.deltaTime);
    }
    public void Finish(bool hit,Vector3 point)
    {
        if(finished) return;
        finished=true;
        if(trail!=null)
        {
            trail.Stop(true,ParticleSystemStopBehavior.StopEmitting);
            trail.transform.SetParent(null,true);
            Destroy(trail.gameObject,0.42f);
        }
        if(hit && impact!=null)
        {
            impact.transform.SetParent(null,true);
            impact.transform.position=point;
            impact.Play(true);
            Destroy(impact.gameObject,0.7f);
        }
    }
}
