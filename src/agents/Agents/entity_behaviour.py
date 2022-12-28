import socket
import sys

from spade.behaviour import FSMBehaviour, CyclicBehaviour

from image_data import ImageData

class AgentBehaviour(FSMBehaviour):
    async def on_start(self):
        print(f"{self.agent.name}: init FSM behaviour.")
        self.command_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.command_socket.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, True) # Send tcp messages instantly
        # self.command_socket.connect(self.agent.command_socket_info)
        self.image_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        # self.image_socket.connect(self.agent.image_socket_info)
        # self.commander = Commander(self, self.command_socket)
        print(f"{self.agent.name}: connected to the server at {self.agent.command_socket_info}.")

    async def on_end(self):
        self.command_socket.close()
        self.image_socket.close()
        print(f"{self.agent.name}: finished FSM behaviour.")
        await self.agent.stop()

class AgentImageBehaviour(CyclicBehaviour):
    async def on_start(self):
        print(f"{self.agent.name}: init image behaviour.")

    async def on_end(self):
        print(f"{self.agent.name}: finished image behaviour.")
        await self.agent.stop()

    async def run(self):
        message = await self.receive(sys.float_info.max)
        if message:
            self.agent.image_queue.put(ImageData(message.body)) 
