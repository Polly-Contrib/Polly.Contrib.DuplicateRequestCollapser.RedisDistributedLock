# Guidelines for contributing

Polly.Contrib hosts contributions around the Polly project by the community.  The project owners have rights on the project repository to merge PRs etc (no review from the Polly team expected or needed, though the team will be happy to take a look if you alert us).


For _process_, the Polly team recommend that Polly-Contrib projects use **Git-Workflow** ([1](https://guides.github.com/introduction/flow/index.html); [2](https://github.com/App-vNext/Polly/wiki/Git-Workflow)) to manage their content:

### Do

+ Keep the `master` branch containing only the code as latest released to NuGet (or code imminently to be released).  
  - Sensible exceptions to this may be eg to improve the `ReadMe` - the main point is that the `master` branch always represents releasable code.
+ Carry out development work in feature branches, usually in your own [fork of the repo](https://help.github.com/en/github/getting-started-with-github/fork-a-repo).
+ Integrate changes into the `Polly-Contrib` `master` branch using PRs.
  - Using PRs allows other developers who may be interested to comment before code is merged (as mentioned above, it is not expected that the Polly team must do this).
  - Using PRs allows developers coming to the project later to see what was added when, and why.

### Avoid

+ Avoid contributing code without PRs.  Committing without PRs makes it hard for others coming to the project later to review what was added when and why.
+ Avoid committing directly to the `master` branch.  Committing interim contributions directly to the `master` branch:
  - means users cannot review the head of the `master` branch as a reference for the published nuget packages;
  - means the `master` branch is not always in an 'immediately releasable' state - this can be important if an urgent bug fix needs to be released;
  - creates unnecessarily messy merge-commits and rebasing, when there are multiple contributors.

### Consider

+ Consider using [milestones](https://help.github.com/en/github/managing-your-work-on-github/about-milestones) matching release numbers, to label PRs included in and issues closed in a release; see the [core Polly project for example](https://github.com/App-vNext/Polly/milestones?state=closed).
