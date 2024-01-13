import os
import sys
import time
import click
import logging
import subprocess
import configparser
from tqdm import tqdm
from art import text2art
from rich.text import Text
from rich.console import Console
from git import Repo, RemoteProgress, GitCommandError

rich_console = Console()
log = logging.getLogger("log")
log.setLevel(logging.INFO)
log.addHandler(logging.StreamHandler(sys.stdout))

global url, branch, directory, verbose, num_dashes, sleep_time

url = "https://github.com/Unity-Technologies/ml-agents.git"
branch = "develop"
directory = "ml-agents"
verbose = True
num_dashes = 91
sleep_time = 0.43

class CloneError(Exception):
    pass
    
class CloneProgress(RemoteProgress):
    def __init__(self):
        super().__init__()
        self.pbar = tqdm()

    def update(self, op_code, cur_count, max_count=None, message=''):
        self.pbar.total = max_count
        self.pbar.n = cur_count
        self.pbar.refresh()

with open("configuration.ini", "w") as f:
    f.write(f"[Configuration]\nurl = {url}\nbranch = {branch}\ndirectory = {directory}\nverbose = {verbose}\nnum_dashes = {num_dashes}\nsleep_time = {sleep_time}\n")

def install_mlagents(path):
    os.chdir(path)
    return subprocess.run(["python", "-m", "pip", "install", "./ml-agents-envs"]).returncode == 0 and subprocess.run(["python", "-m", "pip", "install", "./ml-agents"]).returncode == 0

def graffiti_text(text):
    return text2art(text, font="graffiti")

def clear_screen():
    os.system("cls" if os.name == "nt" else "clear")

def display_text():
    rich_console.print(graffiti_text("Neko Cat Game"), style="red")

def display_rainbow_text():
    rich_console.print(
        Text("=^_^=  =^_^=  C A T  G A M E  R E S E A R C H  =^_^=  N E K O  C A T  G A M E  =^_^=  =^_^=\n"))

def display_dashes():
    rich_console.print("-" * num_dashes, style="white")
    
def git_clone_repo():
    try:
        log.info(f"Cloning... ({url})")
        if not os.path.exists(directory):
            if verbose:
                pbar = tqdm()
                Repo.clone_from(
                    url,
                    directory,
                    branch=branch,
                    progress=CloneProgress()
                )
            else:
                Repo.clone_from(
                    url,
                    directory,
                    branch=branch
                )
            log.info(f"Cloned {branch} branch of ml-agents repository into {directory}")
        else:
            log.warning(f"{directory} directory already exists, skipping cloning")
            log.info(f"Updating {branch} of the ml-agents repository in {directory}")
            repo = Repo(directory)
            repo.git.fetch()
            
    except GitCommandError as e:
        log.error(f"Failed to update or clone {branch} branch of ml-agents repository")
        raise CloneError(e)

def progress_callback(pbar, op_code, cur_count, max_count=None, message=""):
    pbar.total = max_count
    pbar.n = cur_count
    pbar.set_description(message)
    pbar.refresh()
    tqdm.write(f"{op_code}, {cur_count}, {max_count}, {message}")

def sleep_app():
    time.sleep(sleep_time)

def exit_app():
    sleep_app()

def load_config():
    config = configparser.ConfigParser()
    config.read("configuration.ini")
    url = config["Configuration"]["url"]
    branch = config["Configuration"]["branch"]
    directory = config["Configuration"]["directory"]
    verbose = config["Configuration"].getboolean("verbose")
    num_dashes = config["Configuration"].getint("num_dashes")
    sleep_time = config["Configuration"].getfloat("sleep_time")

def main():
    load_config()
    sleep_app()
    clear_screen()
    display_text()
    display_rainbow_text()
    display_dashes()
    sleep_app()
    git_clone_repo()
    sleep_app()
    install_mlagents(directory)
    sleep_app()
    exit_app()

if __name__ == "__main__":
    main()
    