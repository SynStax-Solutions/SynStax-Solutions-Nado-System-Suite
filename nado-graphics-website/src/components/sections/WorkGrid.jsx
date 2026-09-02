import SectionHead from '../ui/SectionHead'
import './WorkGrid.css'

const DEFAULT_PROJECTS = [
  { tag: 'Brand identity — Nova Records', title: 'A label rebrand built for the pit, not the boardroom.', note: 'Wordmark, sleeve system, tour visuals.', color: 'c1' },
  { tag: 'Packaging — Kindred Coffee', title: 'Bags that argue with the shelf.', color: 'c2' },
  { tag: 'Motion — Halcyon', title: 'Onboarding that moves like the product feels.', color: 'c3' },
  { tag: 'Identity + apparel — Fault Line', title: 'A streetwear system built to survive a hundred reprints.', note: 'Logo, type system, drop campaigns.', color: 'c4' },
]

export default function WorkGrid({
  heading = 'Recent work, picked for range.',
  description = "Four projects, four completely different problems. That's the point.",
  projects = DEFAULT_PROJECTS,
}) {
  return (
    <section className="section work-section snap" id="work">
      <div className="wrap">
        <SectionHead heading={heading} description={description} voice />
        <div className="work-grid">
          {projects.map((project) => (
            <div className={`work-card ${project.color}`} key={project.tag}>
              <div className="work-tag">{project.tag}</div>
              <h3>{project.title}</h3>
              {project.note && <p>{project.note}</p>}
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}