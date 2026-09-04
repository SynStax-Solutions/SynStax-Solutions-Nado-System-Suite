import { useState } from 'react'
import Reveal from '../ui/Reveal'
import './Approach.css'

const SERVICE_GROUPS = [
  {
    label: 'Brand & Identity',
    items: ['Branding', 'Logo Design Packages', 'Concept & Design'],
  },
  {
    label: 'Print Collateral',
    items: [
      'Business Cards', 'Flyer Design', 'Poster Design', 'Banner Design',
      'Folder Design', 'Brochure Design', 'Notepads', 'Calendars',
      'Litho & Digital Printing', 'Canvas Printing',
    ],
  },
  {
    label: 'Publications',
    items: ['Publication Design', 'Annual Report Design'],
  },
  {
    label: 'Signage & Large Format',
    items: ['Signage', 'Flag Banners', 'Gazebos'],
  },
  {
    label: 'Promo & Corporate Gifting',
    items: [
      'T-Shirts', 'Caps', 'Magnets', 'Button Badges', 'Lapel Badges',
      'Corporate Gifts', 'Branded USBs', 'Gift Bags', 'Framing',
      'Corporate Invites', 'Licence Discs',
    ],
  },
  {
    label: 'Digital & Motion',
    items: [
      'Web Design', 'App Development', 'Animated Explainer Videos',
      'Social Media Animated Posts',
    ],
  },
]

export default function Approach() {
  const [openIndex, setOpenIndex] = useState(0)

  return (
    <section className="nd-approach" id="approach">
      <div className="wrap">
        <Reveal className="nd-eyebrow">What we actually do</Reveal>

        <Reveal as="h2" className="nd-heading nd-approach-title" delay={80}>
          Concept · Design · Print · Web.
        </Reveal>

        <Reveal as="p" className="nd-lead nd-approach-lead" delay={140}>
          One agency, one point of contact, covering everything from a single logo to a full 
          campus-wide rollout - identity, print, promotional goods, signage, digital and motion, all under one roof.
        </Reveal>

        <Reveal className="nd-accordion" delay={200}>
          {SERVICE_GROUPS.map((group, i) => {
            const isOpen = openIndex === i
            return (
              <div key={group.label} className={`nd-accordion-item ${isOpen ? 'is-open' : ''}`}>
                <button
                  type="button"
                  className="nd-accordion-trigger"
                  onClick={() => setOpenIndex(isOpen ? null : i)}
                  aria-expanded={isOpen}
                >
                  <span className="nd-accordion-title">
                    <span className="nd-accordion-index">{String(i + 1).padStart(2, '0')}</span>
                    {group.label}
                  </span>
                  <span className="nd-accordion-meta">
                    <span className="nd-accordion-count">{group.items.length}</span>
                    <span className="nd-accordion-icon" aria-hidden="true"></span>
                  </span>
                </button>

                <div className="nd-accordion-collapse">
                  <div className="nd-accordion-inner">
                    <div className="nd-accordion-chips">
                      {group.items.map((item) => (
                        <span key={item} className="nd-accordion-chip">{item}</span>
                      ))}
                    </div>
                  </div>
                </div>
              </div>
            )
          })}
        </Reveal>

        <Reveal className="nd-approach-scale" delay={280}>
          <div className="nd-scale-badge">Large-scale delivery</div>
          <h3>Extensive experience with universities and higher education institutions.</h3>
          <p>
            We've delivered branding, print and signage across higher education, 
            from multi-campus rollouts and faculty-wide print runs to the demands of orientation week.
            We've also worked with large corporate clients over many years. 
            When something needs to reach thousands of people at once, we know how to make it happen.
          </p>
        </Reveal>
      </div>
    </section>
  )
}