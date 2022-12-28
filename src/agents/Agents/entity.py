import asyncio
import json
import socket
import sys
import time

from queue import LifoQueue 

from spade.agent import Agent
from spade.message import Message
from spade.template import Template

from entity_behaviour import AgentBehaviour, AgentImageBehaviour
from entity_state import STATE_INIT, STATE_PERCEPTION, STATE_COGNITION, STATE_ACTION
from entity_state import StateInit, StatePerception, StateCognition, StateAction
from commander import Axis

class EntityAgent(Agent):
    def __init__(self, name_at: str, password: str, command_socket_info: tuple, image_socket_info: tuple, image_buffer_size: int, image_folder_name: str, enable_agent_collision: bool, prefab_name: str, starter_position: dict):
        Agent.__init__(self, name_at, password)
        self.command_socket_info = command_socket_info
        self.image_socket_info = image_socket_info
        self.image_buffer_size = image_buffer_size
        self.image_folder_name = image_folder_name
        self.agent_collision = enable_agent_collision
        self.prefab_name = prefab_name
        self.starter_position = starter_position
        self.__server_jit = "simulator@localhost"
        self.camera = True

    async def setup(self):
        fsm_behaviour = AgentBehaviour()

        # STATES
        fsm_behaviour.add_state(name=STATE_INIT, state=StateInit(), initial=True)
        fsm_behaviour.add_state(name=STATE_PERCEPTION, state=StatePerception())
        fsm_behaviour.add_state(name=STATE_COGNITION, state=StateCognition())
        fsm_behaviour.add_state(name=STATE_ACTION, state=StateAction())

        # TRANSITIONS
        fsm_behaviour.add_transition(source=STATE_INIT, dest=STATE_PERCEPTION)
        fsm_behaviour.add_transition(source=STATE_PERCEPTION, dest=STATE_COGNITION)
        fsm_behaviour.add_transition(source=STATE_COGNITION, dest=STATE_ACTION)
        fsm_behaviour.add_transition(source=STATE_ACTION, dest=STATE_PERCEPTION)
        
        # MESSAGE TEMPLATE
        fsm_template = Template()
        fsm_template.set_metadata("simulator", "command")

        # ADD BEHAVIOUR
        self.add_behaviour(fsm_behaviour, fsm_template)
        print(f"{self.name}: FSM behaviour is ready.")

        # ADD IMAGE BEHAVIOUR IF AGENT'S AVATAR HAS A CAMERA
        if self.camera:
            image_behaviour = AgentImageBehaviour()
            image_template = Template()
            image_template.set_metadata("simulator", "image")
            self.image_queue = LifoQueue()
            self.add_behaviour(image_behaviour, image_template)


    async def send_msg_to_server_and_wait(self, msg:str) -> str:
        """
        Send a message and waits for a response
        """
        # encoded_msg = (msg).encode()
        # self.__command_socket.sendall(bytearray(encoded_msg))
        # return self.__command_socket.recv(128)
        message = Message(to=self.__server_jit)
        message.body = msg
        message.set_metadata("simulator", "command")
        await self.behaviours[0].send(message)
        reply = await self.behaviours[0].receive(sys.float_info.max)
        if reply:
            return reply.body
        return None

    async def send_command_to_server_and_wait(self, msg:dict) -> str:
        """
        Send a command and waits for a response
        """
        # encoded_msg = json.dumps(msg).encode()
        # self.__command_socket.sendall(bytearray(encoded_msg))
        # return self.__command_socket.recv(128)
        await self.send_command_to_server(msg)
        reply = await self.behaviours[0].receive(sys.float_info.max)
        if reply:
            return reply.body
        return None


    async def send_command_to_server(self, msg:dict):
        """
        Send a command 
        """
        # encoded_msg = json.dumps(msg).encode()
        # self.__command_socket.sendall(bytearray(encoded_msg))
        encoded_msg = json.dumps(msg)
        message = Message(to=self.__server_jit)
        message.body = encoded_msg
        message.set_metadata("simulator", "command")
        await self.behaviours[0].send(message)


    async def create_agent(self) -> list:
        command = { 'commandName': 'create', 'data': [self.name, self.prefab_name] }
        position = self.starter_position
        if isinstance(position, str):
            command['data'].append(position)
        else:
            command['data'].append(f"({position['x']} {position['y']} {position['z']})")
        command['data'].append(self.agent_collision)
        self.position = (await self.send_command_to_server_and_wait(command)) # .decode('utf-8')
        return [float(x) for x in (self.position.split())[1:]]

    async def move_agent(self, position: list) -> list:
        command = { 'commandName': 'moveTo', 'data': [position] }
        msg = (await self.send_command_to_server_and_wait(command)) # .decode('utf-8')
        new_position = [float(x) for x in (msg.split())[1:]]
        return new_position

    async def fov_camera(self, camera_id: int, fov: float):
        data = [ f"{camera_id}", f"{fov}" ]
        cameraRotateCommand = { 'commandName': 'cameraFov', 'data': data }
        await self.send_command_to_server(cameraRotateCommand)

    async def move_camera(self, camera_id: int, axis: Axis, relative_position: float):
        data = [ f"{camera_id}", f"{axis}", f"{relative_position}" ]
        cameraRotateCommand = { 'commandName': 'cameraMove', 'data': data }
        await self.send_command_to_server(cameraRotateCommand)

    async def rotate_camera(self, camera_id: int, axis: Axis, degrees: float):
        data = [ f"{camera_id}", f"{axis}", f"{degrees}" ]
        cameraRotateCommand = { 'commandName': 'cameraRotate', 'data': data }
        await self.send_command_to_server(cameraRotateCommand)

    async def take_image(self, camera_id: int, image_mode: float):
        command = { 'commandName': 'image', 'data': [ f"{camera_id}", f"{image_mode}" ] }
        await self.send_command_to_server(command)

    async def change_color(self, r: float, g: float, b: float, a: float = 1):
        ''' Color must be normalized between [0, 1]'''
        color = { 'r': r, 'g': g, 'b': b, 'a': a }
        color_string = json.dumps(color)
        command = { 'commandName': 'color', 'data': [ color_string ] }
        await self.send_command_to_server(command)
