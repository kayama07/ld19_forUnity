using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // 1�b���Destroy���\�b�h���Ăяo���āA���g��j������
        Invoke("DestroySelf", 0.1f);
    }

    // ���g��j�����郁�\�b�h
    void DestroySelf()
    {
        Destroy(gameObject);
    }
}

