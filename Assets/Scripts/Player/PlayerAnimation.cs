using Unity.Netcode;
using UnityEngine;

public class PlayerAnimation : NetworkBehaviour
{
    private Animator _playerAnimator;
    private PlayerInputControls _playerInputControls;

    private AnimatorControllerParameter[] allParams;


    public override void OnNetworkSpawn()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerAnimator = GetComponentInChildren<Animator>();
            _playerInputControls = GetComponent<PlayerInputControls>();

            allParams = _playerAnimator.parameters;


            _playerInputControls.OnMoveInput += PlayerInputControlsOnOnMoveInput;
            _playerInputControls.OnMoveActionCancelled += PlayerInputControlsOnOnMoveActionCancelled;
            _playerInputControls.OnShootInput += PlayerInputControlsOnOnShootInput;
            _playerInputControls.OnShootInputCancelled += PlayerInputControlsOnOnShootInputCancelled;


        }
    }

    private void PlayerInputControlsOnOnShootInputCancelled()
    {
        SetOneParameterToTrue(param: "isIdle");
    }

    private void PlayerInputControlsOnOnShootInput()
    {
        SetOneParameterToTrue(param:"isShooting");
    }

    private void PlayerInputControlsOnOnMoveActionCancelled()
    {
        SetOneParameterToTrue(param:"isIdle");
    }

    private void PlayerInputControlsOnOnMoveInput(Vector3 context)
    {
        if (context.magnitude > 0)
        {
            SetOneParameterToTrue(param:"isRunning");
        }
    }


    void SetOneParameterToTrue(string param)
    {
        foreach (var parameter in allParams)
        {
            if (parameter.name == param)
            {
                _playerAnimator.SetBool(parameter.nameHash, true);
            }
            else
            {
                _playerAnimator.SetBool(parameter.nameHash, false);

            }

        }
    }

    public override void OnNetworkDespawn()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerInputControls.OnMoveInput -= PlayerInputControlsOnOnMoveInput;
            _playerInputControls.OnMoveActionCancelled -= PlayerInputControlsOnOnMoveActionCancelled;
            _playerInputControls.OnShootInput -= PlayerInputControlsOnOnShootInput;
            _playerInputControls.OnShootInputCancelled -= PlayerInputControlsOnOnShootInputCancelled;
        }
    }
}