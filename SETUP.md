# Git setup
git clone --recurse-submodules https://github.com/maxlep/MeatScape-Adventure.git

git config --global diff.submodule log

git config --global status.submodulesummary 1

git config --global submodule.recurse 1

git config push.recurseSubmodules on-demand

# To make changes to the submodule
1. cd Assets/AssetStore/

2. git checkout master

3. Make changes

4. git submodule update --remote --merge
