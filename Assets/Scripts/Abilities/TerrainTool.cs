using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnCreature : Abilities
{
    private InputSystem_Actions _cameraActions;
    private Camera _camera;

    [SerializeField] private GameObject slime;

    private void Start()
    {
        _camera = Camera.main;
    }

    void Update()
    {
        if(isUsing)
            Capacity();
    }

    void OnEnable()
    {
        _cameraActions.Player.UseAbility.performed += SpawnSlime;
    }

    void SpawnSlime(InputAction.CallbackContext context)
    {
        bool isSpawningSlime = context.performed;
        if (isSpawningSlime)
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hit);
            Instantiate(slime,hit.point,Quaternion.identity);
        }
    }

    public override void Capacity()
    {
    }

    public override void StartUsing()
    {
        isUsing = true;
    }

    public override void StopUsing()
    {
        isUsing = false;
    }
}