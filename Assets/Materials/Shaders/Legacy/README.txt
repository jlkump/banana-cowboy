Documentation for shader properties in the cel-shader since Unity doesn't let you set tooltips

Color: Base color

Ambient Color: Supposed to be the ambient light reflected onto the model. Affects all shades of the model

Rim Color: Illuminating color applied to the edges of the model to simulate backlighting or reflected light.
           Its mostly to help the model stand out more.

Glossiness: This is a number that affects the specular component of the blinn-phong shading model.
            Basically is the really reflective part of glossy surfaces.

            The default value is bugged and is always 0.5, this causes the whole model to be
            rendered as the glossy part of the model. The default value should be 32, a lower
            value makes the model look MORE glossy. To make the material appear more matte, pick
            a really large number. My favorite is 100000000000.

Rim Intensity: This number controls how wide the "rim" is. Like with glossiness,
               a lower value indicates a higher intensity.

Rim Spread:    This number indicates how far the rim should spread along the edge of the mesh.
               Like with glossiness, a lower value indicates more spread.