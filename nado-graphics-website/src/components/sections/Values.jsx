import Reveal from '../ui/Reveal'
import './Values.css'

const VALUES = [
  {
    num: '01',
    title: 'Communicate',
    desc: 'Every project starts with a conversation. We listen, ask questions and get to the heart of what you are trying to achieve.',
    grad: 'nd-grad-1',
  },
  {
    num: '02',
    title: 'Collaborate',
    desc: 'We work alongside you throughout the process — sharing ideas, refining directions and making decisions together.',
    grad: 'nd-grad-2',
  },
  {
    num: '03',
    title: 'Create',
    desc: 'Once the direction is right, we bring it to life through thoughtful design and quality print.',
    grad: 'nd-grad-3',
  },
]

export default function Values() {
  return (
    <section className="nd-values" id="values">
      <div className="wrap">
        <Reveal className="nd-eyebrow">The process</Reveal>

        <Reveal as="h2" className="nd-heading nd-values-title" delay={80}>
          How we work.
        </Reveal>

        <div className="nd-values-grid">
          {VALUES.map((v, i) => (
            <Reveal key={v.num} className="surface-card nd-value-card" delay={160 + i * 100}>
              <span className={`nd-value-num ${v.grad}`}>{v.num}</span>
              <h3>{v.title}</h3>
              <p>{v.desc}</p>
            </Reveal>
          ))}
        </div>
      </div>
    </section>
  )
}
