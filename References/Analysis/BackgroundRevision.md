# Alley boundary and background revision

Revised the existing Alley scene without rebuilding the original architecture or changing URP.

- Added continuous ground beneath both frontages, passage floors, foundations, service-slot return walls, neighboring building masses and north/south continuation geometry.
- The east side passage now has walls, paving, a drain, a light and a visible closed service gate. Other narrow service slots terminate against actual geometry.
- Replaced the active procedural skyline with the supplied Ian Muttoo Tokyo photograph, using a UV crop focused on Tokyo Tower. The photo is sampled directly; the original JPEG remains unchanged. Shader settings enlarge/reposition the crop and blend its gray sky into a deep-blue procedural sky with sparse stars and subtle cloud variation.
- The tower is a distant photographic background element, not a 3D model. Most of the photograph's city/river is deliberately concealed. The bright tower region is soft at this enlarged scale because it occupies a small part of the source panorama.
- This is a limited-angle prototype background, not a full 360-degree equirectangular capture or geographically calibrated city simulation. Final panoramic art and a higher-detail tower source remain future improvements.
- Updated the reflection probe so the street no longer reflects the previous skyline.

## Checks
338 downward samples across the street plus 10 along the side passage found no missing ground. 1,872 horizontal rays at player height from positions along the street all hit enclosing geometry within 100 m. These are sampled checks, supplemented by player-height image review, not an exhaustive proof for every camera position and angle. No Console errors were reported after compilation/capture.

Updated 1600 x 1000 images are in References/Captures/Revision. All use the player camera at approximately 1.7 m above the road; some views look upward while keeping eye height unchanged.

Photo: Tokyo At Night, Panorama by Ian Muttoo, CC BY-SA 2.0. Source requested: https://commons.wikimedia.org/wiki/File:Tokyo_at_night_panorama.jpg . The same original-resolution photo was downloaded from the author's Flickr CDN after Wikimedia returned rate-limit errors. Full attribution, transformations and license link are in Assets/Alley/Textures/Tokyo Photo License.txt. The adapted photographic background is CC BY-SA 2.0.
