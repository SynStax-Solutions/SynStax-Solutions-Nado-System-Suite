import SectionHead from '../ui/SectionHead'
import './About.css'

const DEFAULT_STATS = [
  { value: '60+', label: 'Brands shipped' },
  { value: '7 yrs', label: 'In business' },
  { value: '4', label: 'Disciplines, done properly' },
]

export default function About({
  heading = 'A small studio, on purpose.',
  description = 'Four people, one point of view, no account managers between you and the work.',
  body = "We keep the team small so every project gets senior hands, start to finish. That means fewer clients at a time, and more attention on the ones we take on.",
  stats = DEFAULT_STATS,
}) {
  return (
    <section className="section about-section snap" id="about">
      <div className="wrap">
        <SectionHead heading={heading} description={description} voice />
        <div className="about-body">
          <p>{body}</p>
          <div className="about-stats">
            {stats.map((stat) => (
              <div className="about-stat" key={stat.label}>
                <div className="about-stat-value">{stat.value}</div>
                <div className="about-stat-label">{stat.label}</div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  )
}