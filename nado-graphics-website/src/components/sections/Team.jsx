import Reveal from '../ui/Reveal'
import './Team.css'

const STATS = [
  { value: '3+', label: 'Universities & higher ed institutions' },
  { value: '40+', label: 'Brands shipped since 2000' },
  { value: 'Nationwide', label: 'Large-scale, multi-campus rollouts' },
]

// Placeholders — replace with real clients / logos once recevied
const CLIENTS = [
  { name: 'University of the Western Cape', category: 'Higher Education' },
  { name: 'University of Cape Town', category: 'Higher Education' },
  { name: 'University of Pretoria', category: 'Higher Education' },
  { name: 'University of Stellenbosch', category: 'Higher Education' },
  { name: 'Melomed Hospital', category: 'Healthcare & Medical' },
  { name: 'CancerCare', category: 'Healthcare & Medical' },

]

export default function Team() {
  return (
    <section className="nd-team" id="clients">
      <div className="wrap">
        <Reveal className="nd-eyebrow">Trusted by</Reveal>

        <Reveal as="h2" className="nd-heading nd-team-title" delay={80}>
          Big clients we've worked with.
        </Reveal>

        <Reveal as="p" className="nd-lead nd-team-lead" delay={140}>
          From university campuses to national corporates, we've delivered
          branding, print and signage at a scale most studios our size
          never see.
        </Reveal>

        <div className="nd-team-stats">
          {STATS.map((s, i) => (
            <Reveal key={s.label} className="surface-card nd-team-stat" delay={180 + i * 80}>
              <span>{s.value}</span>
              <p>{s.label}</p>
            </Reveal>
          ))}
        </div>

        <div className="nd-team-grid">
          {CLIENTS.map((client, i) => (
            <Reveal key={client.name} className="surface-card nd-client-card" delay={260 + i * 60}>
              <span className="nd-client-category">{client.category}</span>
              <h3>{client.name}</h3>
            </Reveal>
          ))}
        </div>
      </div>
    </section>
  )
}