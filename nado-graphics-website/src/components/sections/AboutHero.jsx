// src/components/sections/AboutHero.jsx
import Reveal from '../ui/Reveal'
import './AboutHero.css'

const CHIPS = [
  { label: 'Identity', tint: 'chip-violet' },
  { label: 'Packaging', tint: 'chip-coral' },
  { label: 'Motion', tint: 'chip-blue' },
  { label: 'Art Direction', tint: 'chip-cyan' },
]

export default function AboutHero() {
  return (
    <section className="nd-hero" id="about-hero">
      <div className="nd-hero-shape" aria-hidden="true"></div>
      <div className="nd-hero-shape-two" aria-hidden="true"></div>

      <div className="wrap nd-hero-inner">
        <Reveal className="nd-eyebrow">Design & Print Agency, est. 2000</Reveal>

        <Reveal as="h1" className="nd-heading nd-hero-title" delay={80}>
           <span className="nd-grad-text">Communicate. Collaborate. Create.</span>
        </Reveal>

        <Reveal as="p" className="nd-lead nd-hero-lead" delay={160}>
          Nado Graphics is a design and print agency built from a lifelong passion for the craft. 
          From the first idea to the final printed piece, we combine experience, creativity 
          and attention to detail to bring brands to life.
        </Reveal>

        <Reveal className="nd-hero-ctas" delay={240}>
          <a href="/portfolio" className="nd-btn nd-btn-primary">See the work</a>
          <a href="/contact" className="nd-btn nd-btn-ghost">Start a project</a>
        </Reveal>

        <div className="nd-hero-chips">
          {CHIPS.map((c, i) => (
            <Reveal key={c.label} className={`nd-chip ${c.tint}`} delay={320 + i * 80}>
              {c.label}
            </Reveal>
          ))}
        </div>
      </div>
    </section>
  )
}
