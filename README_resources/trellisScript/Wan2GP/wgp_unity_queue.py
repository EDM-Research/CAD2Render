import json
import os
import sys
import tempfile
import zipfile

if len(sys.argv) > 1:
    json_data = [{
        "id": 1,
        "params": {
            "image_mode": 1,
            "prompt": sys.argv[1],
            "alt_prompt": "",
            "negative_prompt": "",
            "resolution": "1024x1024",
            "video_length": 81,
            "duration_seconds": 0,
            "batch_size": 1,
            "seed": -1,
            "force_fps": "",
            "num_inference_steps": 30,
            "guidance_scale": 4,
            "guidance2_scale": 5,
            "guidance3_scale": 5,
            "switch_threshold": 0,
            "switch_threshold2": 0,
            "guidance_phases": 1,
            "model_switch_phase": 1,
            "alt_guidance_scale": 6,
            "audio_guidance_scale": 4,
            "audio_scale": 1,
            "flow_shift": 6,
            "sample_solver": "",
            "embedded_guidance_scale": 6,
            "repeat_generation": 1,
            "multi_prompts_gen_type": 0,
            "multi_images_gen_type": 0,
            "skip_steps_cache_type": "",
            "skip_steps_multiplier": 1.75,
            "skip_steps_start_step_perc": 0,
            "loras_multipliers": "",
            "image_prompt_type": "",
            "image_start": None,
            "image_end": None,
            "model_mode": None,
            "video_source": None,
            "keep_frames_video_source": "",
            "input_video_strength": 1.0,
            "video_guide_outpainting": "",
            "video_prompt_type": "",
            "image_refs": None,
            "frames_positions": None,
            "video_guide": None,
            "image_guide": None,
            "keep_frames_video_guide": "",
            "denoising_strength": 1.0,
            "masking_strength": 1.0,
            "video_mask": None,
            "image_mask": None,
            "control_net_weight": 1,
            "control_net_weight2": 1,
            "control_net_weight_alt": 1,
            "motion_amplitude": 1.0,
            "mask_expand": 0,
            "audio_guide": None,
            "audio_guide2": None,
            "custom_guide": None,
            "audio_source": None,
            "audio_prompt_type": "",
            "speakers_locations": "0:45 55:100",
            "sliding_window_size": 81,
            "sliding_window_overlap": 5,
            "sliding_window_color_correction_strength": 0,
            "sliding_window_overlap_noise": 0,
            "sliding_window_discard_last_frames": 0,
            "image_refs_relative_size": 50,
            "remove_background_images_ref": 1,
            "temporal_upsampling": "",
            "spatial_upsampling": "",
            "film_grain_intensity": 0,
            "film_grain_saturation": 0.5,
            "MMAudio_setting": 0,
            "MMAudio_prompt": "",
            "MMAudio_neg_prompt": "",
            "RIFLEx_setting": 0,
            "NAG_scale": 1,
            "NAG_tau": 3.5,
            "NAG_alpha": 0.5,
            "slg_switch": 0,
            "slg_layers": [
                9
            ],
            "slg_start_perc": 10,
            "slg_end_perc": 90,
            "apg_switch": 0,
            "cfg_star_switch": 0,
            "cfg_zero_step": -1,
            "prompt_enhancer": "",
            "min_frames_if_references": 1,
            "override_profile": -1,
            "override_attention": "",
            "pace": 0.5,
            "exaggeration": 0.5,
            "temperature": 0.8,
            "top_k": 50,
            "output_filename": "",
            "mode": "",
            "activated_loras": [],
            "model_type": "z_image_base",
            "settings_version": 2.44,
            "base_model_type": "z_image_base"
        }
    }]

    with tempfile.TemporaryDirectory() as tmpdir:
        manifest_path = os.path.join(tmpdir, "queue.json")
        try:
            with open(manifest_path, 'w', encoding='utf-8') as f:
                json.dump(json_data, f, indent=4)
        except Exception as e:
            print(f"Error writing queue.json: {e}")
            exit(1)

        try:
            # create folder if necessary
            output = sys.argv[2] if len(sys.argv) > 2 else "outputs/queue.zip"
            testDestination = os.path.dirname(output)
            if not os.path.exists(testDestination):
                os.makedirs(testDestination)
            with zipfile.ZipFile(output, 'w', zipfile.ZIP_DEFLATED) as zf:
                zf.write(manifest_path, arcname="queue.json")
            exit(0)
        except Exception as e:
            print(f"Error creating zip: {e}")
            exit(1)
exit(1)

