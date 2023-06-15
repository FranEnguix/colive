import aioxmpp
import json
import os
import time
# from queue import LifoQueue 
# from datetime import datetime

# from image_data import ImageData
from spade.behaviour import State

# --------------------------------------------- #
# FLAMAS BEHAVIOUR                              #
# --------------------------------------------- #
STATE_FLAMAS_SETUP = "STATE_FLAMAS_SETUP"
STATE_FLAMAS_RECEIVE = "STATE_FLAMAS_RECEIVE"
STATE_FLAMAS_TRAIN = "STATE_FLAMAS_TRAIN"
STATE_FLAMAS_SEND = "STATE_FLAMAS_SEND" 


class StateFlamasSetup(State):
    def __init__(self, shell):
        super().__init__()
        self.__shell = shell

    async def run(self):
        print(f"{self.agent.name}: state {STATE_FLAMAS_SETUP}.")
        await self.__shell.flamas_setup(self.agent)
        role = self.agent.flamas_role
        # assert not role or (role != "client" and role != "server"), "Flamas agent must be either client or server"
        if role == "server":
            self.agent.flamas_models_weights = None
            self.agent.flamas_clients = []
            self.agent.presence.on_subscribe = self.join_listener_behaviour_callback
            self.agent.presence.on_available = self.availability_listener_callback
        else:
            self.agent.presence.subscribe(self.agent.flamas_server)
            self.agent.presence.on_subscribe = self.server_asked_subscription_listener_callback

        self.set_next_state(STATE_FLAMAS_RECEIVE)


    def server_asked_subscription_listener_callback(self, agent_jid: str) -> None:
        self.approve(agent_jid)
        if agent_jid.startswith(self.agent.flamas_server):
            pass

    def join_listener_behaviour_callback(self, agent_jid: str) -> None:
        self.approve(agent_jid)
        self.agent.flamas_clients.append(agent_jid)
        self.agent.presence.subscribe(agent_jid)

    def availability_listener_callback(self, agent_jid: str, stanza: aioxmpp.Presence) -> None:
        pass

class StateFlamasReceive(State):
    def __init__(self, shell):
        super().__init__()
        self.__shell = shell

    async def run(self):
        print(f"{self.agent.name}: state {STATE_FLAMAS_RECEIVE}.")
        await self.__shell.flamas_receive(self.agent)
        if self.agent.flamas_role == "server":
            while not self.agent.flamas_clients:
                time.sleep(1)
            # clients_with_weights = [client for client in self.agent.flamas_clients if client not in self.agent.flamas_new_clients]
            self.agent.flamas_models_weights = await self.wait_for_client_weights(self.agent.flamas_clients)
        else:
            self.agent.presence.set_presence(
                state = axiompp.PresenceState(True, PresenceShow.CHAT),
                status = "flamas_weights",
                )
            self.agent.flamas_weights = await self.wait_for_server_average(self.agent.flamas_server)
        self.set_next_state(STATE_FLAMAS_TRAIN)

    async def wait_for_client_weights(self, clients: list[str]) -> list[list[float]]:
        client_weights = []
        client_replies = []
        while len(client_replies) < len(clients):
            # receive code
            reply = await self.receive(sys.float_info.max)
            if reply:
                client_weights.append(json.loads(reply.body))
                client_replies.append(reply.sender)
        return client_weights

    async def wait_for_server_average(self, server_jid: str) -> list[float]:
        reply = await self.receive(sys.float_info.max)
        if reply:
            return json.loads(reply.body)
        return None

class StateFlamasTrain(State):
    def __init__(self, shell):
        super().__init__()
        self.__shell = shell

    async def run(self):
        print(f"{self.agent.name}: state {STATE_FLAMAS_TRAIN}.")
        if self.agent.flamas_role == "server" and self.agent.flamas_models_weights:
            self.agent.flamas_weights = await self.compute_average(self.agent.flamas_model_weights)
        await self.__shell.flamas_train(self.agent)
        self.set_next_state(STATE_FLAMAS_SEND)

    async def compute_average(self, models_weights: list[list[float]]) -> list[float]:
        if not models_weights:
            return []
        num_models = len(models_weights)
        num_weights = len(models_weights[0])
        model_weights_cummulative = [0] * num_weights
        for vector_weights in models_weights:
            for i in range(num_weights):
                model_weights_cummulative += vector_weights
        model_weights_mean = [weight / num_weights for weight in model_weights_cummulative]
        return model_weights_mean


class StateFlamasSend(State):
    def __init__(self, shell):
        super().__init__()
        self.__shell = shell

    async def run(self):
        print(f"{self.agent.name}: state {STATE_FLAMAS_SEND}.")
        await self.__shell.flamas_send(self.agent)
        if self.agent.flamas_role == "server":
            await self.send_weights(self.agent.flamas_weights, self.agent.flamas_clients)
        else:
            await self.send_weights(self.agent.flamas_weights, [self.agent.flamas_server])
        self.set_next_state(STATE_FLAMAS_RECEIVE)

    async def send_weights(self, weights: list[float], agents: list[str]) -> None:
        if not weights:
            print(f"{self.agent.name} flamas: has not weights to send") 
        else:
            encoded_msg = json.dumps(weights)
            for agent in agents:
                message = Message(to=agent)
                message.body = encoded_msg
                message.set_metadata("flamas", "weights")
                await self.agent.dispatch(message)


