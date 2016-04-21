using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using mattatz.GeneticAlgorithm;
using mattatz.NeuralNetworks;

using mattatz.Utils;

namespace mattatz.EvolvingVirtualCreatures {

	public class Factory : MonoBehaviour {

		Population population;
		[SerializeField] Text generationLabel;

		[SerializeField] float size = 5f;
		[SerializeField] int count = 50;

		[SerializeField] GameObject bodyPrefab;
		[SerializeField] GameObject segmentPrefab;

		[SerializeField] float mutationRate = 0.2f;

		[SerializeField] bool automatic = true;
		[SerializeField] float interval = 40f;

		void Start () {
			Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Creature"), LayerMask.NameToLayer("Creature"));

			population = new Population(mutationRate);

			int hcount = Mathf.FloorToInt(count * 0.5f);
			for(int i = 0; i < count; i++) {
				var position = new Vector3((i - hcount) * size, 0.5f, 0f);
				SampleCreature creature = CreateCreature(i.ToString(), position);
				population.AddCreature(creature);
			}

			population.Setup();

			if(automatic) {
				StartCoroutine(Repeat(interval));
			}

		}

		IEnumerator Repeat(float interval) {
			yield return 0;

			interval = Mathf.Clamp (interval, 30f, 90f);

			while(true) {
				yield return new WaitForSeconds(interval);
				Reproduction();
			}
		}

		void Update () {
			population.Work();
			population.ComputeFitness();
		}

		public void Reproduction () {
			population.ComputeFitness();

			var ancestors = new List<Creature>(population.Creatures);
			int hcount = Mathf.FloorToInt(ancestors.Count * 0.5f);

			population.Reproduction((DNA dna, int index, int count) => {
				var position = new Vector3((index - hcount) * size, 0.5f, 0f);
				return CreateCreature(index.ToString(), position, dna);
			}, new Vector2(-1f, 1f));

			ancestors.ForEach(ancestor => {
				var group = (ancestor as SampleCreature).Body.transform.parent;
				Destroy(group.gameObject);
			});

			population.Setup();
			generationLabel.text = population.Generations.ToString();
		}

		SampleCreature CreateCreature (string label, Vector3 position, DNA dna = null) {
			var rightArm = Instantiate(segmentPrefab).GetComponent<Segment>();
			var leftArm = Instantiate(segmentPrefab).GetComponent<Segment>();
			var body = Instantiate(bodyPrefab).GetComponent<Body>();

			body.transform.position = position;
			rightArm.Connect(body.Body, SideType.Right);
			leftArm.Connect(body.Body, SideType.Left);
			body.AddSegment(rightArm);
			body.AddSegment(leftArm);

			var group = new GameObject(label);
			group.transform.position = position;
			body.transform.parent = group.transform;
			rightArm.transform.parent = group.transform;
			leftArm.transform.parent = group.transform;

			if(dna == null) {
				return new SampleCreature(body);
			}

			return new SampleCreature(body, dna);
		}


	}

}
