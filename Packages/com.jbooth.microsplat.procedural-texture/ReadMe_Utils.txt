CPU side utilities for determining the texture at a give location are included. 

A version for Jobs is included in a zip file, so that it won't cause compile errors on load
as it requires the new mathematics and collection libraries from Unity. 


The jobs version requires you to construct a Texture8 and Texture16 object from the texture 
data the module produces using texture.GetRawData as the NativeArray pointer. 

The returned data in both cases is 4 weight values and 4 texture indexes, which you can corrilate
with the texture arrays or texture array config data. 
