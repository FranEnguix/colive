from email.message import Message
import time
import os

from commander import Axis, ImageMode
from spade.agent import Agent
from spade.message import Message

from image_data import ImageData

class EntityShell:

    async def init(agent: Agent):
        time.sleep(4)

    async def perception(agent: Agent, data: ImageData):
        await agent.change_color(1, 0, 0, 0.8)
        time.sleep(4)

    async def cognition(agent: Agent):
        await agent.change_color(0, 1, 0, 0.8)
        time.sleep(4)

    async def action(agent: Agent):
        await agent.change_color(0, 0, 1, 0.8)
        time.sleep(4)