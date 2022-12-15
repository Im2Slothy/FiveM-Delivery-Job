using System.Collections.Generic;
using System.Linq;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace FiveM_Scripts
{
    public class DeliveryMission : BaseScript
    {
        private const int DeliveryItemModel = -774712033; // Model of the delivery item
        private const int RewardAmount = 1000; // Amount of money rewarded for completing the mission

        private List<Vector3> _deliveryLocations = new List<Vector3>()
        {
            new Vector3(441.135f, -1020.068f, 29.000f), // Location 1
            new Vector3(-1068.976f, -1270.828f, 5.000f), // Location 2
            new Vector3(-1319.917f, -1289.122f, 4.000f), // Location 3
            new Vector3(-1667.844f, -1129.938f, 2.000f) // Location 4
        };

        private int _currentLocationIndex = 0;
        private bool _isMissionActive = false;
        private bool _isDeliveryItemSpawned = false;
        private Blip _missionBlip;

        public DeliveryMission()
        {
            EventHandlers["onClientMapStart"] += new Action(OnClientMapStart);
            EventHandlers["onPlayerDeath"] += new Action<int>(OnPlayerDeath);
            EventHandlers["onPlayerWasted"] += new Action<int>(OnPlayerDeath);
        }

        private void OnClientMapStart()
        {
            // Set up blip for the mission
            _missionBlip = API.AddBlipForCoord(_deliveryLocations[0].X, _deliveryLocations[0].Y, _deliveryLocations[0].Z);
            API.SetBlipSprite(_missionBlip, 1);
            API.SetBlipDisplay(_missionBlip, 2);
            API.SetBlipScale(_missionBlip, 0.5f);
            API.SetBlipColour(_missionBlip, 1);
            API.SetBlipAsShortRange(_missionBlip, true);
            API.BeginTextCommandSetBlipName("STRING");
            API.AddTextComponentString("Delivery Mission");
            API.EndTextCommandSetBlipName(_missionBlip);
        }

        private void OnPlayerDeath()
        {
            if (_isMissionActive)
            {
                // If player dies during the mission, reset the mission
                ResetMission();
            }
        }

        [Command("startdelivery")]
        private void StartDeliveryMissionCommand(int source, List<object> args, string raw)
        {
            if (!_isMissionActive)
            {
                // Start the mission
                _isMissionActive = true;
                _currentLocationIndex = 0;
                TriggerEvent("chat:addMessage", new
                {
                    color = new[] { 255, 0, 0 },
                    args = new[] { "Delivery Mission", "A delivery mission has been started. Deliver the item to all the locations to complete the mission." }
                });
                SpawnDeliveryItem();
            }
            else
            {
                TriggerEvent("chat:addMessage", new
{
color = new[] { 255, 0, 0 },
args = new[] { "Delivery Mission", "A delivery mission is already active. Finish or reset the current mission before starting a new one." }
});
}
}
    [Command("resetdelivery")]
    private void ResetDeliveryMissionCommand(int source, List<object> args, string raw)
    {
        if (_isMissionActive)
        {
            // Reset the current mission
            ResetMission();
        }
        else
        {
            TriggerEvent("chat:addMessage", new
            {
                color = new[] { 255, 0, 0 },
                args = new[] { "Delivery Mission", "There is no active delivery mission to reset." }
            });
        }
    }

    private void ResetMission()
    {
        _isMissionActive = false;
        _isDeliveryItemSpawned = false;
        _currentLocationIndex = 0;
        API.RemoveBlip(_missionBlip);
        TriggerEvent("chat:addMessage", new
        {
            color = new[] { 255, 0, 0 },
            args = new[] { "Delivery Mission", "The current delivery mission has been reset." }
        });
    }

    private void SpawnDeliveryItem()
    {
        if (_isMissionActive && !_isDeliveryItemSpawned)
        {
            // Spawn the delivery item at the player's current location
            Vector3 playerPosition = API.GetEntityCoords(API.PlayerPedId(), false, false);
            int item = API.CreateObject(DeliveryItemModel, playerPosition.X, playerPosition.Y, playerPosition.Z, true, true, true);
            API.PlaceObjectOnGroundProperly(item);
            _isDeliveryItemSpawned = true;
            TriggerEvent("chat:addMessage", new
            {
                color = new[] { 255, 0, 0 },
                args = new[] { "Delivery Mission", "The delivery item has been spawned. Take it to the next location to complete the mission." }
            });
        }
        else if (!_isMissionActive)
        {
            TriggerEvent("chat:addMessage", new
            {
                color = new[] { 255, 0, 0 },
                args = new[] { "Delivery Mission", "There is no active delivery mission. Start a new mission before spawning the delivery item." }
            });
        }
        else
        {
            TriggerEvent("chat:addMessage", new
            {
                color = new[] { 255, 0, 0 },
                args = new[] { "Delivery Mission", "The delivery item has already been spawned. Take it to the next location to complete the mission." }
            });
        }
    }

    private async Task CheckForDeliveryCompletion()
    {
        while (_isMissionActive)
        {
            Vector3 playerPosition = API.GetEntityCoords(API.PlayerPedId(), false, false);
            Vector3 deliveryLocation = _deliveryLocations[_currentLocationIndex];
            if (API.Vdist(playerPosition.X, playerPosition.Y, playerPosition.Z, deliveryLocation.X, deliveryLocation.Y, deliveryLocation.Z) < 10.0f)
            {
                if (_currentLocationIndex == _deliveryLocations.Count - 1)
                {
                    // If player has reached the last delivery location, give them the reward and reset the mission
API.GivePlayerMoney(RewardAmount);
TriggerEvent("chat:addMessage", new
{
color = new[] { 255, 0, 0 },
args = new[] { "Delivery Mission", $"Congratulations, you have successfully delivered the item to all locations! You have been rewarded with {RewardAmount}$." }
});
ResetMission();
}
else
{
// If player has reached a delivery location but it is not the last one, update the blip and message the player
_currentLocationIndex++;
API.SetBlipCoords(_missionBlip, _deliveryLocations[_currentLocationIndex].X, _deliveryLocations[_currentLocationIndex].Y, _deliveryLocations[_currentLocationIndex].Z);
TriggerEvent("chat:addMessage", new
{
color = new[] { 255, 0, 0 },
args = new[] { "Delivery Mission", "You have successfully delivered the item to the current location. Take it to the next location to complete the mission." }
});
}
}
            await Delay(1000);
        }
    }
}
}