# The Candidate 

The candidate is a simple VR application that focuses on simulating different job interviews.

The project is in collaboration of NAV where the goal is for people to use it in practicing before job interviews and become more secure and comfortable.

The project is stitched together by the use of many already existing open source project out there, like

- The makehuman community for creation of humans
- Ulipsync for the lip syncing
- Poly haven for assets

## Developer stack

The following project uses a variety of different applications to develop. The full prerequisite is as follow:
- Unity Hub (Main editor + Unity Version Control)
- GIT with LFS (LFS required for textures, 3D models or AI models)
- Blender (Display some 3D files within Unity and modify already existing)
    - Make Human plugin (Used for creation of avatars)


## Environmental Variables
This projects uses some environmental variables to hide them from the repo. These can be set globally within your machine or use the following method.

### Using .env file
After Installing unity hub and cloning the project, copy the `.env.example` file and set the prerequisite keys. After that, run either the `Start.command` for UNIX systems (Linux & MAC) or the `Start.ps1`.

## Feedback after the interview
In the interview, the pause menu has an «Avslutt intervju» button. It ends the interview and shows feedback from an observer agent: what went well and what can be better, each point with a quote of what the job seeker said. The observer is the shared agent from the agent configurator, and the rules for reading its reply are copied from there (`ObserverFeedback.cs`). Nothing is saved: the conversation and the feedback are gone when the job seeker goes back to the start.

To turn it on:
- Set `ObserverAgentId` on the `PublishedRag` provider asset to the observer agent's id. It is not a secret.
- On RAGdoll, set `CANDIDATE_OBSERVER_AGENT_ID` to the same id, so the `CANDIDATE` client key may chat with it.
