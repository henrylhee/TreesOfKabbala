#ifndef GETCELLTYPECOLOR_INCLUDED
#define GETCELLTYPECOLOR_INCLUDED

void GetCellTypeColor_float(float noise, float3 earth, float3 stone, float3 grass, float3 water, float3 energy, out float3 output)
{
	if(noise >= 0.0f && noise < 0.2f)
	{ 
		output = earth;
	}
	if(noise >= 0.2f && noise < 0.4f)
	{ 
		output = stone;
	}
	if(noise >= 0.4f && noise < 0.6f)
	{
		output = grass;
	}
	if(noise >= 0.6f && noise < 0.8f)
	{ 
		output = water;
	}
	if(noise >= 0.8f && noise <= 1.0f)
	{ 
		output = energy;
	}
}

#endif // GETCELLTYPECOLOR_INCLUDED