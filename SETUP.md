# Git setup
git clone --recurse-submodules https://github.com/maxlep/MeatScape-Adventure.git

git config --global diff.submodule 1

git config --global status.submodulesummary 1

git config --global submodule.recurse 1

git config push.recurseSubmodules on-demand

# To make changes to the submodule
cd AssetStore/

git checkout master

## Make changes
git submodule update --remote --merge
