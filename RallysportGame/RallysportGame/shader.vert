#version 130

in vec3 position;
in vec3 normalIn;

out vec3 viewSpaceNormal; 
out vec3 viewSpacePosition;

uniform mat4 normalMatrix;
uniform mat4 modelViewMatrix;
uniform mat4 modelViewProjectionMatrix; 

uniform mat4 lightMatrix;

void main() 
{
	gl_Position = modelViewProjectionMatrix * vec4(position,1.0);
	viewSpacePosition = (modelViewMatrix * vec4(position, 1.0)).xyz;
	viewSpaceNormal = normalize( (normalMatrix * vec4(normalIn, 0.0) ).xyz); 

}
