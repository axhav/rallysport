#version 130

// required by GLSL spec Sect 4.5.3 (though nvidia does not, amd does)
precision highp float;

in vec3 viewSpaceNormal; 
in vec3 viewSpacePosition;

uniform vec3 lightPosition;
uniform vec3 scene_ambient_light = vec3(0.05, 0.05, 0.05);
uniform vec3 scene_light = vec3(0.7, 0.7, 0.7);

uniform vec3 material_diffuse_color;
uniform vec3 material_specular_color = vec3(0.1); 
uniform vec3 material_emissive_color = vec3(0.0);
uniform float material_shininess = 25.0;
uniform mat4 modelViewMatrix;

uniform bool show_specular;

out vec4 fragmentColor;

vec3 calculateAmbient(vec3 ambientLight, vec3 materialAmbient)
{
	return ambientLight * materialAmbient;
}

vec3 calculateDiffuse(vec3 diffuseLight, vec3 materialDiffuse, vec3 normal, vec3 directionToLight)
{
	return diffuseLight * materialDiffuse * max(0, dot(normal, directionToLight));
}

vec3 calculateSpecular(vec3 specularLight, vec3 materialSpecular, float materialShininess, vec3 normal, vec3 directionToLight, vec3 directionFromEye)
{
	vec3 h = normalize(directionToLight - directionFromEye);
	float normalizationFactor = ((materialShininess + 2.0) / 8.0);
	return (specularLight * materialSpecular * pow(max(0, dot(h, normal)), materialShininess))*normalizationFactor;
}

vec3 calculateFresnel(vec3 materialSpecular, vec3 normal, vec3 directionFromEye)
{
	return materialSpecular + (vec3(1.0) - materialSpecular) * pow(clamp(1.0 + dot(directionFromEye, normal), 0.0, 1.0), 5.0);
}

void main() 
{
	vec3 viewSpaceLightPosition = (modelViewMatrix * vec4(lightPosition, 1.0)).xyz;
	vec3 normal = normalize(viewSpaceNormal);
	vec3 directionToLight = normalize( viewSpaceLightPosition - viewSpacePosition );
	vec3 directionFromEye = normalize( viewSpacePosition );
	vec3 specular = calculateSpecular(scene_light, material_specular_color, material_shininess, normal, directionToLight, directionFromEye);
	vec3 shading = calculateAmbient(scene_ambient_light, material_diffuse_color) + 
			calculateDiffuse(scene_light, material_diffuse_color, normal, directionToLight) +
			0.3*specular;

	//fragmentColor = show_specular ? vec4(shading, 1.0) : vec4(material_diffuse_color, 1.0);
	fragmentColor = vec4(1,0,0,0);
}
